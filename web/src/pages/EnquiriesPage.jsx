import { useEffect, useMemo, useState } from "react";
import { enquiryService } from "../services/api";
import { DataTable } from "../components/ui/DataTable";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "../components/ui/Feedback";
import { Icon } from "../components/ui/Icon";
import { PageHeader } from "../components/ui/PageHeader";
import { StatusBadge } from "../components/ui/StatusBadge";

function formatDate(value) {
  return new Intl.DateTimeFormat("es-DO", {
    day: "numeric",
    month: "short",
    year: "numeric",
  }).format(new Date(value));
}

export function EnquiriesPage() {
  const [result, setResult] = useState({ key: null, data: [], error: "" });
  const [searchValue, setSearchValue] = useState("");
  const [search, setSearch] = useState("");
  const [type, setType] = useState("");
  const [selectedEnquiry, setSelectedEnquiry] = useState(null);
  const [reloadKey, setReloadKey] = useState(0);
  const isLoading = result.key !== reloadKey;
  const enquiries = result.data;
  const error = result.error;

  useEffect(() => {
    const controller = new AbortController();
    enquiryService
      .getPending(controller.signal)
      .then((data) => setResult({ key: reloadKey, data, error: "" }))
      .catch((requestError) => {
        if (requestError.name !== "AbortError") {
          setResult({ key: reloadKey, data: [], error: requestError.message });
        }
      });
    return () => controller.abort();
  }, [reloadKey]);

  const types = useMemo(
    () =>
      [
        ...new Set(
          enquiries.map((enquiry) => enquiry.tipoSeguro).filter(Boolean),
        ),
      ].sort(),
    [enquiries],
  );
  const filteredEnquiries = useMemo(
    () =>
      enquiries.filter((enquiry) => {
        const searchTerm = search.toLocaleLowerCase("es");
        const matchesSearch =
          !searchTerm ||
          `${enquiry.cliente?.nombre ?? ""} ${enquiry.cliente?.correo ?? ""} ${enquiry.tipoSeguro ?? ""}`
            .toLocaleLowerCase("es")
            .includes(searchTerm);
        return matchesSearch && (!type || enquiry.tipoSeguro === type);
      }),
    [enquiries, search, type],
  );

  const columns = [
    {
      key: "cliente",
      label: "Cliente",
      render: (enquiry) => (
        <span className="table-person">
          <span className="table-avatar">
            {enquiry.cliente?.nombre?.[0] ?? "C"}
          </span>
          <span className="table-person-copy">
            <strong>{enquiry.cliente?.nombre ?? "Cliente"}</strong>
            <small>{enquiry.cliente?.correo ?? ""}</small>
          </span>
        </span>
      ),
    },
    { key: "tipoSeguro", label: "Tipo de seguro" },
    {
      key: "fechaSolicitud",
      label: "Fecha",
      render: (enquiry) => formatDate(enquiry.fechaSolicitud),
    },
    {
      key: "estado",
      label: "Estado",
      render: (enquiry) => <StatusBadge status={enquiry.estado} />,
    },
    {
      key: "presupuesto",
      label: "Presupuesto",
      render: (enquiry) =>
        enquiry.presupuesto == null
          ? "Por definir"
          : `RD$ ${new Intl.NumberFormat("es-DO").format(enquiry.presupuesto)}`,
    },
  ];

  function submitSearch(event) {
    event.preventDefault();
    setSearch(searchValue.trim());
  }

  return (
    <>
      <PageHeader
        eyebrow="CADA SOLICITUD ES UNA OPORTUNIDAD"
        title="Solicitudes"
        description="Da seguimiento a las solicitudes de asesoría que esperan atención."
      />
      <section className="content-card directory-card">
        <div className="directory-toolbar">
          <div className="directory-count">
            <strong>{isLoading ? "—" : filteredEnquiries.length}</strong>
            <span>solicitudes pendientes</span>
          </div>
          <div className="directory-filters">
            <form
              className="search-form enquiry-search"
              onSubmit={submitSearch}
              role="search"
            >
              <Icon name="search" size={18} />
              <label className="sr-only" htmlFor="enquiry-search">
                Buscar solicitudes
              </label>
              <input
                id="enquiry-search"
                type="search"
                placeholder="Buscar cliente o seguro"
                value={searchValue}
                onChange={(event) => setSearchValue(event.target.value)}
              />
              <button
                className="button button-secondary search-button"
                type="submit"
              >
                Buscar
              </button>
            </form>
            <label className="sr-only" htmlFor="enquiry-type">
              Filtrar por tipo de seguro
            </label>
            <select
              id="enquiry-type"
              className="filter-select"
              value={type}
              onChange={(event) => setType(event.target.value)}
            >
              <option value="">Todos los seguros</option>
              {types.map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </div>
        </div>

        {isLoading ? (
          <LoadingState label="Cargando solicitudes..." />
        ) : error ? (
          <ErrorState
            message={error}
            onRetry={() => setReloadKey((key) => key + 1)}
          />
        ) : filteredEnquiries.length === 0 ? (
          <EmptyState
            title={
              enquiries.length
                ? "No hay coincidencias"
                : "No hay solicitudes pendientes"
            }
            description={
              enquiries.length
                ? "Ajusta la búsqueda o el filtro para ver otros resultados."
                : "Las nuevas solicitudes aparecerán aquí para que puedas darles seguimiento."
            }
          />
        ) : (
          <DataTable
            columns={columns}
            rows={filteredEnquiries}
            getRowKey={(enquiry) => enquiry.id}
            onRowClick={setSelectedEnquiry}
          />
        )}
        {!isLoading && !error && filteredEnquiries.length > 0 && (
          <div className="table-footer">
            Mostrando {filteredEnquiries.length} de {enquiries.length}{" "}
            solicitudes
          </div>
        )}
      </section>

      {selectedEnquiry && (
        <aside
          className="detail-panel"
          aria-label={`Solicitud de ${selectedEnquiry.cliente?.nombre ?? "cliente"}`}
        >
          <div className="detail-panel-top">
            <span className="table-avatar detail-avatar">
              {selectedEnquiry.cliente?.nombre?.[0] ?? "C"}
            </span>
            <button
              className="icon-button"
              onClick={() => setSelectedEnquiry(null)}
              aria-label="Cerrar detalle"
            >
              <Icon name="close" />
            </button>
          </div>
          <p className="eyebrow">
            DETALLE DE SOLICITUD · #{selectedEnquiry.id}
          </p>
          <h2>{selectedEnquiry.cliente?.nombre ?? "Cliente"}</h2>
          <div className="detail-field">
            <span>Tipo de seguro</span>
            <strong>{selectedEnquiry.tipoSeguro}</strong>
          </div>
          <div className="detail-field">
            <span>Estado</span>
            <StatusBadge status={selectedEnquiry.estado} />
          </div>
          <div className="detail-field">
            <span>Fecha de solicitud</span>
            <strong>{formatDate(selectedEnquiry.fechaSolicitud)}</strong>
          </div>
          <div className="detail-field">
            <span>Presupuesto</span>
            <strong>
              {selectedEnquiry.presupuesto == null
                ? "Por definir"
                : `RD$ ${new Intl.NumberFormat("es-DO").format(selectedEnquiry.presupuesto)}`}
            </strong>
          </div>
          {selectedEnquiry.descripcionNecesidad && (
            <div className="detail-description">
              <span>Necesidad del cliente</span>
              <p>{selectedEnquiry.descripcionNecesidad}</p>
            </div>
          )}
          <div className="detail-contact">
            <Icon name="calendar" size={18} />
            <span>
              {selectedEnquiry.horario?.fecha
                ? `Horario reservado: ${selectedEnquiry.horario.fecha}`
                : "Pendiente de coordinar"}
            </span>
          </div>
        </aside>
      )}
    </>
  );
}
