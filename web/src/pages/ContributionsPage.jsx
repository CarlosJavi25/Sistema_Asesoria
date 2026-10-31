import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { advisoryService, enquiryService, quoteService } from "../services/api";
import { EmptyState, LoadingState } from "../components/ui/Feedback";
import { Icon } from "../components/ui/Icon";
import { PageHeader } from "../components/ui/PageHeader";
import { StatusBadge } from "../components/ui/StatusBadge";

function formatCurrency(value) {
  return `RD$ ${new Intl.NumberFormat("es-DO", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)}`;
}

function formatDate(value) {
  return new Intl.DateTimeFormat("es-DO", {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(new Date(value));
}

export function ContributionsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const advisoryId = searchParams.get("asesoriaId");
  const selectedProductParams = searchParams.getAll("productoId").join(",");
  const selectedUrlIds = useMemo(
    () => selectedProductParams.split(",").map(Number).filter((id) => Number.isInteger(id) && id > 0),
    [selectedProductParams],
  );
  const [quotes, setQuotes] = useState([]);
  const [advisories, setAdvisories] = useState([]);
  const [advisoriesLoaded, setAdvisoriesLoaded] = useState(false);
  const [comparison, setComparison] = useState({ advisoryId: null, alternatives: [], error: "" });
  const [selectedIds, setSelectedIds] = useState(selectedUrlIds);
  const [premiums, setPremiums] = useState({});
  const [notes, setNotes] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  const selectedAdvisorySummary = advisories.find(
    (item) => String(item.asesoria.id) === advisoryId,
  );
  const selectedAdvisory = selectedAdvisorySummary?.asesoria ?? null;
  const alternatives = useMemo(
    () => comparison.advisoryId === advisoryId ? comparison.alternatives : [],
    [comparison, advisoryId],
  );
  const comparisonError = comparison.advisoryId === advisoryId
    ? comparison.error
    : "";
  const invalidAdvisory = Boolean(advisoryId && advisoriesLoaded && !selectedAdvisory);
  const selectedAlternatives = useMemo(
    () => alternatives.filter((item) => selectedIds.includes(item.productoId)),
    [alternatives, selectedIds],
  );
  const totalPremium = selectedAlternatives.reduce(
    (total, item) => total + (Number(premiums[item.productoId]) || 0),
    0,
  );

  useEffect(() => {
    const controller = new AbortController();
    Promise.all([
      quoteService.getMine(controller.signal),
      advisoryService.getMine(controller.signal),
    ])
      .then(([quoteResults, advisoryResults]) => {
        setQuotes(quoteResults);
        setAdvisories(advisoryResults);
      })
      .catch((requestError) => {
        if (requestError.name !== "AbortError") setError(requestError.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) {
          setIsLoading(false);
          setAdvisoriesLoaded(true);
        }
      });
    return () => controller.abort();
  }, [reloadKey]);

  useEffect(() => {
    if (!advisoryId || !selectedAdvisory) return undefined;

    const controller = new AbortController();
    enquiryService.getComparison(selectedAdvisory.solicitudId, controller.signal)
      .then((products) => {
        setComparison({ advisoryId, alternatives: products, error: "" });
        setSelectedIds(selectedUrlIds);
        setPremiums((current) => {
          const next = { ...current };
          products.forEach((product) => {
            if (next[product.productoId] === undefined) {
              next[product.productoId] = String(product.primaCotizada);
            }
          });
          return next;
        });
      })
      .catch((requestError) => {
        if (requestError.name !== "AbortError") {
          setComparison({ advisoryId, alternatives: [], error: requestError.message });
        }
      });
    return () => controller.abort();
  }, [advisoryId, selectedAdvisory, selectedUrlIds]);

  const isComparisonLoading = Boolean(
    advisoryId
    && advisoriesLoaded
    && selectedAdvisory
    && comparison.advisoryId !== advisoryId
    && !comparisonError,
  );

  function chooseAdvisory(event) {
    const id = event.target.value;
    if (id) setSearchParams({ asesoriaId: id });
  }

  function updatePremium(productId, value) {
    setPremiums((current) => ({ ...current, [productId]: value }));
  }

  async function createQuote(event) {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (!selectedAdvisory || selectedAlternatives.length === 0) {
      setError("Selecciona al menos una alternativa para preparar la cotización.");
      return;
    }

    const invalidPremium = selectedAlternatives.some((item) => {
      const value = Number(premiums[item.productoId]);
      return premiums[item.productoId] === "" || !Number.isFinite(value) || value < 0;
    });
    if (invalidPremium) {
      setError("Revisa las primas: deben ser montos válidos mayores o iguales a cero.");
      return;
    }

    setIsSaving(true);
    try {
      const createdQuote = await quoteService.create(selectedAdvisory.id, {
        detalles: selectedAlternatives.map((item) => ({
          productoId: item.productoId,
          primaCotizada: Number(premiums[item.productoId]),
        })),
        observaciones: notes.trim() || null,
      });

      setSuccess(`La cotización #${createdQuote.id} quedó guardada como borrador.`);
      setSearchParams({});
      setNotes("");
      setReloadKey((key) => key + 1);
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsSaving(false);
    }
  }

  async function presentQuote(quoteId) {
    setError("");
    setSuccess("");
    try {
      const updatedQuote = await quoteService.changeState(quoteId, "Presentada");
      setQuotes((current) => current.map((item) => item.cotizacion.id === quoteId
        ? { ...item, cotizacion: updatedQuote }
        : item));
      setSuccess(`La cotización #${quoteId} fue marcada como presentada.`);
    } catch (requestError) {
      setError(requestError.message);
    }
  }

  if (isLoading) return <LoadingState label="Cargando cotizaciones..." />;
  if (isComparisonLoading) return <LoadingState label="Cargando alternativas seleccionadas..." />;

  return (
    <>
      <PageHeader
        eyebrow="PROPUESTAS A LA MEDIDA"
        title="Aportes y cotizaciones"
        description="Prepara propuestas claras después de la asesoría y consulta el historial de cotizaciones."
      />

      {error && <div className="inline-alert" role="alert">{error}</div>}
      {success && <div className="inline-success" role="status">{success}</div>}
      {comparisonError && <div className="inline-alert" role="alert">{comparisonError}</div>}
      {invalidAdvisory && (
        <div className="inline-alert" role="alert">
          No encontramos esa asesoría. Elige una asesoría disponible en la lista para continuar.
        </div>
      )}

      {advisoryId && selectedAdvisory ? (
        <section className="content-card quote-builder">
          <div className="card-heading">
            <div>
              <p className="eyebrow">NUEVA PROPUESTA</p>
              <h2>{selectedAdvisorySummary.clienteNombre} · {selectedAdvisorySummary.tipoSeguro}</h2>
            </div>
            <Link className="text-link" to={`/app/comparador?solicitudId=${selectedAdvisory.solicitudId}`}>
              Editar comparación <Icon name="arrow" size={16} />
            </Link>
          </div>
          {selectedAlternatives.length === 0 ? (
            <EmptyState
              title="No hay alternativas seleccionadas"
              description="Vuelve al comparador y selecciona al menos un producto para continuar."
            />
          ) : (
            <form onSubmit={createQuote}>
              <div className="quote-items">
                {selectedAlternatives.map((alternative) => (
                  <article className="quote-item" key={alternative.productoId}>
                    <div className="quote-item-product">
                      <span className="quote-item-insurer">{alternative.producto.aseguradora.nombre}</span>
                      <strong>{alternative.producto.nombre}</strong>
                      <small>{alternative.producto.tipoSeguro.nombre} · {alternative.producto.nivelCobertura}</small>
                    </div>
                    <label className="premium-field">
                      <span>Prima cotizada</span>
                      <span className="premium-input-wrap">
                        <span>RD$</span>
                        <input
                          type="number"
                          min="0"
                          step="0.01"
                          inputMode="decimal"
                          value={premiums[alternative.productoId] ?? ""}
                          onChange={(event) => updatePremium(alternative.productoId, event.target.value)}
                          aria-label={`Prima cotizada para ${alternative.producto.nombre}`}
                          required
                        />
                      </span>
                    </label>
                  </article>
                ))}
              </div>
              <label className="notes-field">
                <span>Observaciones para la propuesta</span>
                <textarea
                  rows="3"
                  maxLength="4000"
                  value={notes}
                  onChange={(event) => setNotes(event.target.value)}
                  placeholder="Incluye detalles importantes para presentar esta propuesta al cliente."
                />
              </label>
              <div className="quote-total">
                <span>Total de primas seleccionadas</span>
                <strong>{formatCurrency(totalPremium)}</strong>
              </div>
              <div className="quote-form-actions">
                <Link className="button button-secondary" to={`/app/comparador?solicitudId=${selectedAdvisory.solicitudId}`}>
                  Volver al comparador
                </Link>
                <button className="button button-primary" type="submit" disabled={isSaving}>
                  {isSaving ? "Guardando..." : "Guardar borrador"} {!isSaving && <Icon name="arrow" size={17} />}
                </button>
              </div>
            </form>
          )}
        </section>
      ) : (
        <section className="content-card contribution-start">
          <div className="contribution-start-copy">
            <span className="contribution-start-icon"><Icon name="quote" size={22} /></span>
            <div>
              <p className="eyebrow">INICIAR COTIZACIÓN</p>
              <h2>Prepara una propuesta para tu cliente</h2>
              <p>Selecciona una asesoría ya realizada. Podrás comparar las opciones, ajustar las primas y guardar el borrador.</p>
            </div>
          </div>
          <label htmlFor="quote-advisory">Asesoría realizada</label>
          <div className="quote-request-select">
            <select id="quote-advisory" className="filter-select" defaultValue="" onChange={chooseAdvisory}>
              <option value="">Selecciona un cliente y tipo de seguro</option>
              {advisories.map(({ asesoria, clienteNombre, tipoSeguro }) => (
                <option key={asesoria.id} value={asesoria.id}>
                  {clienteNombre} · {tipoSeguro} · {formatDate(asesoria.fechaAsesoria)}
                </option>
              ))}
            </select>
            {advisories.length === 0 && (
              <span>Cuando registres una asesoría realizada, podrás preparar una cotización aquí.</span>
            )}
          </div>
        </section>
      )}

      <section className="quotes-section">
        <div className="card-heading">
          <div><p className="eyebrow">HISTORIAL</p><h2>Cotizaciones recientes</h2></div>
          <span className="quotes-count">{quotes.length} {quotes.length === 1 ? "cotización" : "cotizaciones"}</span>
        </div>
        {quotes.length === 0 ? (
          <section className="content-card">
            <EmptyState
              title="Todavía no hay cotizaciones"
              description="Cuando prepares la primera propuesta, aparecerá aquí para que puedas consultarla y presentarla."
            />
          </section>
        ) : (
          <div className="quotes-list">
            {quotes.map(({ cotizacion, clienteNombre, tipoSeguro }) => {
              const total = cotizacion.detalles.reduce((sum, item) => sum + item.primaCotizada, 0);
              return (
                <article className="quote-history-card" key={cotizacion.id}>
                  <div className="quote-history-main">
                    <span className="quote-history-icon"><Icon name="quote" size={20} /></span>
                    <div>
                      <span className="quote-history-id">COT-{String(cotizacion.id).padStart(4, "0")} · {formatDate(cotizacion.fechaEmision)}</span>
                      <strong>{clienteNombre}</strong>
                      <small>{tipoSeguro} · {cotizacion.detalles.length} {cotizacion.detalles.length === 1 ? "alternativa" : "alternativas"}</small>
                    </div>
                  </div>
                  <div className="quote-history-total">
                    <span>Primas cotizadas</span>
                    <strong>{formatCurrency(total)}</strong>
                  </div>
                  <StatusBadge status={cotizacion.estado} />
                  {cotizacion.estado === "Borrador" && (
                    <button className="button button-secondary" type="button" onClick={() => presentQuote(cotizacion.id)}>
                      Marcar presentada
                    </button>
                  )}
                  {cotizacion.observaciones && <p className="quote-history-notes">{cotizacion.observaciones}</p>}
                </article>
              );
            })}
          </div>
        )}
      </section>
    </>
  );
}
