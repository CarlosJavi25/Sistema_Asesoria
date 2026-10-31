import { useEffect, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { advisoryService, enquiryService } from "../services/api";
import { EmptyState, ErrorState, LoadingState } from "../components/ui/Feedback";
import { Icon } from "../components/ui/Icon";
import { PageHeader } from "../components/ui/PageHeader";

function formatCurrency(value) {
  return `RD$ ${new Intl.NumberFormat("es-DO", {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(value)}`;
}

function getCoverageLabel(level) {
  return {
    Basico: "Básico",
    Intermedio: "Intermedio",
    Completo: "Completo",
  }[level] ?? level;
}

export function ComparatorPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const requestId = searchParams.get("solicitudId");
  const [requests, setRequests] = useState([]);
  const [request, setRequest] = useState(null);
  const [advisory, setAdvisory] = useState(null);
  const [alternatives, setAlternatives] = useState([]);
  const [selectedIds, setSelectedIds] = useState([]);
  const [result, setResult] = useState({ key: "", isLoading: true, error: "" });
  const loadKey = requestId ?? "requests";

  useEffect(() => {
    const controller = new AbortController();
    if (!requestId) {
      enquiryService.getPending(controller.signal)
        .then((data) => {
          setRequests(data);
          setResult({ key: loadKey, isLoading: false, error: "" });
        })
        .catch((error) => {
          if (error.name !== "AbortError") {
            setResult({ key: loadKey, isLoading: false, error: error.message });
          }
        });
      return () => controller.abort();
    }

    Promise.all([
      enquiryService.getById(requestId, controller.signal),
      enquiryService.getComparison(requestId, controller.signal),
    ])
      .then(async ([selectedRequest, products]) => {
        setRequest(selectedRequest);
        setAlternatives(products);
        setSelectedIds(products.map((product) => product.productoId));
        try {
          setAdvisory(await advisoryService.getByRequest(requestId, controller.signal));
        } catch (error) {
          if (error.status !== 404) throw error;
          setAdvisory(null);
        }
        setResult({ key: loadKey, isLoading: false, error: "" });
      })
      .catch((error) => {
        if (error.name !== "AbortError") {
          setResult({ key: loadKey, isLoading: false, error: error.message });
        }
      });
    return () => controller.abort();
  }, [requestId, loadKey]);

  const isLoading = result.key !== loadKey || result.isLoading;
  const selectedAlternatives = useMemo(
    () => alternatives.filter((alternative) => selectedIds.includes(alternative.productoId)),
    [alternatives, selectedIds],
  );
  const quoteUrl = useMemo(() => {
    const params = new URLSearchParams({ asesoriaId: String(advisory?.id ?? "") });
    selectedIds.forEach((id) => params.append("productoId", String(id)));
    return `/app/aportes?${params.toString()}`;
  }, [advisory?.id, selectedIds]);

  function chooseRequest(event) {
    const value = event.target.value;
    if (value) setSearchParams({ solicitudId: value });
  }

  function toggleAlternative(productId) {
    setSelectedIds((current) => current.includes(productId)
      ? current.filter((id) => id !== productId)
      : [...current, productId]);
  }

  if (isLoading) return <LoadingState label="Preparando la comparación..." />;
  if (result.error) {
    return <ErrorState message={result.error} onRetry={() => setSearchParams(requestId ? { solicitudId: requestId } : {})} />;
  }

  return (
    <>
      <PageHeader
        eyebrow="DECISIONES CON MÁS CLARIDAD"
        title="Comparador de seguros"
        description="Revisa coberturas, primas y beneficios para recomendar la opción que mejor se ajusta al cliente."
      />

      {!requestId ? (
        <section className="content-card comparator-picker">
          <label htmlFor="comparison-request">Selecciona una solicitud pendiente</label>
          <select id="comparison-request" className="filter-select" defaultValue="" onChange={chooseRequest}>
            <option value="">Elige un cliente y tipo de seguro</option>
            {requests.map((item) => (
              <option key={item.id} value={item.id}>
                {item.cliente?.nombre ?? "Cliente"} · {item.tipoSeguro} · #{item.id}
              </option>
            ))}
          </select>
          {requests.length === 0 && (
            <EmptyState
              title="No hay solicitudes para comparar"
              description="Las solicitudes pendientes de tus clientes aparecerán aquí."
            />
          )}
        </section>
      ) : (
        <>
          <section className="comparison-request content-card">
            <span className="comparison-request-icon"><Icon name="people" size={20} /></span>
            <div className="comparison-request-info">
              <span>Solicitud #{request.id} · {request.tipoSeguro}</span>
              <strong>{request.cliente?.nombre ?? "Cliente"}</strong>
              <small>{request.presupuesto == null ? "Presupuesto por definir" : `Presupuesto: ${formatCurrency(request.presupuesto)}`}</small>
            </div>
            <Link className="text-link" to="/app/solicitudes">Ver solicitudes <Icon name="arrow" size={16} /></Link>
          </section>

          {alternatives.length === 0 ? (
            <section className="content-card">
              <EmptyState
                title="No hay alternativas vigentes"
                description="Todavía no existen productos activos para este tipo de seguro."
              />
            </section>
          ) : (
            <>
              <div className="comparison-toolbar">
                <p>{alternatives.length} opciones disponibles <span>·</span> {selectedIds.length} seleccionadas</p>
                <button
                  type="button"
                  className="button button-secondary"
                  onClick={() => setSelectedIds(alternatives.map((item) => item.productoId))}
                >
                  Seleccionar todas
                </button>
              </div>

              <section className="comparison-grid" aria-label="Alternativas de seguro">
                {alternatives.map((alternative) => {
                  const product = alternative.producto;
                  const isSelected = selectedIds.includes(alternative.productoId);
                  return (
                    <article className={`comparison-card${isSelected ? " comparison-card-selected" : ""}`} key={alternative.productoId}>
                      <div className="comparison-card-top">
                        <label className="comparison-select">
                          <input
                            type="checkbox"
                            checked={isSelected}
                            onChange={() => toggleAlternative(alternative.productoId)}
                          />
                          <span>Incluir</span>
                        </label>
                        {alternative.recomendada && <span className="recommended-badge"><Icon name="shield" size={14} /> Recomendado</span>}
                      </div>
                      <p className="comparison-insurer">{product.aseguradora.nombre}</p>
                      <h2>{product.nombre}</h2>
                      <div className="comparison-price">{formatCurrency(alternative.primaCotizada)}<small> prima estimada</small></div>
                      <div className="comparison-score">
                        <span>Ajuste con la solicitud</span>
                        <strong>{alternative.puntajeCoincidencia ?? 0}<small>/100</small></strong>
                      </div>
                      <dl className="comparison-facts">
                        <div><dt>Cobertura</dt><dd>{getCoverageLabel(product.nivelCobertura)}</dd></div>
                        <div><dt>Deducible</dt><dd>{formatCurrency(product.deducible)}</dd></div>
                      </dl>
                      {product.beneficiosAdicionales && (
                        <div className="comparison-benefits">
                          <strong>Beneficios</strong>
                          <p>{product.beneficiosAdicionales}</p>
                        </div>
                      )}
                      <details className="coverage-details">
                        <summary>{product.coberturas.length} coberturas incluidas</summary>
                        {product.coberturas.length ? (
                          <ul>{product.coberturas.map((coverage) => <li key={coverage.id}>{coverage.descripcion}</li>)}</ul>
                        ) : <p>Consulta las condiciones con la aseguradora.</p>}
                      </details>
                    </article>
                  );
                })}
              </section>
              <div className="comparison-footer">
                <span>
                  {advisory
                    ? "La prima final está sujeta a evaluación de la aseguradora."
                    : "Registra la asesoría una vez finalizada la cita para poder preparar una cotización."}
                </span>
                {advisory ? (
                  <Link
                    className="button button-primary"
                    aria-disabled={selectedAlternatives.length === 0}
                    tabIndex={selectedAlternatives.length === 0 ? -1 : undefined}
                    onClick={(event) => {
                      if (selectedAlternatives.length === 0) event.preventDefault();
                    }}
                    to={quoteUrl}
                  >
                    Preparar cotización <Icon name="arrow" size={17} />
                  </Link>
                ) : (
                  <Link className="button button-secondary" to="/app/solicitudes">
                    Ir a solicitudes <Icon name="arrow" size={17} />
                  </Link>
                )}
              </div>
            </>
          )}
        </>
      )}
    </>
  );
}
