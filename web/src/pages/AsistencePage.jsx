import { useEffect, useMemo, useState } from "react";
import { assistanceService } from "../services/api";
import { DataTable } from "../components/ui/DataTable";
import { EmptyState, ErrorState, LoadingState } from "../components/ui/Feedback";
import { Icon } from "../components/ui/Icon";
import { PageHeader } from "../components/ui/PageHeader";
import { StatusBadge } from "../components/ui/StatusBadge";
import { useAuth } from "../context/useAuth";

function formatDate(value) {
  return new Intl.DateTimeFormat("es-DO", {
    day: "numeric",
    month: "short",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(value));
}

export function AsistencePage() {
  const { user } = useAuth();
  const [requests, setRequests] = useState([]);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [searchValue, setSearchValue] = useState("");
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [isLoading, setIsLoading] = useState(true);
  const [isUpdating, setIsUpdating] = useState(false);
  const [error, setError] = useState("");
  const [actionError, setActionError] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    assistanceService.getForStaff(controller.signal)
      .then(setRequests)
      .catch((requestError) => {
        if (requestError.name !== "AbortError") setError(requestError.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false);
      });
    return () => controller.abort();
  }, [reloadKey]);

  const filteredRequests = useMemo(() => requests.filter((request) => {
    const searchTerm = search.toLocaleLowerCase("es");
    const searchContent = `${request.cliente?.nombre ?? ""} ${request.cliente?.correo ?? ""} ${request.motivo ?? ""}`
      .toLocaleLowerCase("es");
    return (!searchTerm || searchContent.includes(searchTerm))
      && (!status || request.estado === status);
  }), [requests, search, status]);

  const columns = [
    {
      key: "cliente",
      label: "Cliente",
      render: (request) => (
        <span className="table-person">
          <span className="table-avatar">{request.cliente?.nombre?.[0] ?? "C"}</span>
          <span className="table-person-copy">
            <strong>{request.cliente?.nombre ?? "Cliente"}</strong>
            <small>{request.cliente?.correo ?? ""}</small>
          </span>
        </span>
      ),
    },
    {
      key: "motivo",
      label: "Motivo",
      render: (request) => <span className="assistance-reason">{request.motivo}</span>,
    },
    {
      key: "fechaSolicitud",
      label: "Fecha",
      render: (request) => formatDate(request.fechaSolicitud),
    },
    {
      key: "asesorNombre",
      label: "Asesor asignado",
      render: (request) => request.asesorNombre || "Sin asignar",
    },
    {
      key: "estado",
      label: "Estado",
      render: (request) => <StatusBadge status={request.estado} />,
    },
  ];

  function submitSearch(event) {
    event.preventDefault();
    setSearch(searchValue.trim());
  }

  function updateRequest(updatedRequest) {
    setRequests((current) => current.map((request) => request.id === updatedRequest.id
      ? updatedRequest
      : request));
    setSelectedRequest(updatedRequest);
  }

  async function performAction(action) {
    setIsUpdating(true);
    setActionError("");
    try {
      const updatedRequest = action === "assign"
        ? await assistanceService.assign(selectedRequest.id)
        : await assistanceService.changeState(selectedRequest.id, "Resuelta");
      updateRequest(updatedRequest);
    } catch (requestError) {
      setActionError(requestError.message);
    } finally {
      setIsUpdating(false);
    }
  }

  return (
    <>
      <PageHeader
        eyebrow="ACOMPAÑAMIENTO Y SEGUIMIENTO"
        title="Solicitudes de asistencia"
        description="Consulta todas las solicitudes de ayuda, revisa su estado y da seguimiento a las asignadas."
      />
      <section className="content-card directory-card">
        <div className="directory-toolbar">
          <div className="directory-count">
            <strong>{isLoading ? "—" : filteredRequests.length}</strong>
            <span>solicitudes de asistencia</span>
          </div>
          <div className="directory-filters">
            <form className="search-form enquiry-search" onSubmit={submitSearch} role="search">
              <Icon name="search" size={18} />
              <label className="sr-only" htmlFor="assistance-search">Buscar solicitudes</label>
              <input
                id="assistance-search"
                type="search"
                placeholder="Buscar cliente o motivo"
                value={searchValue}
                onChange={(event) => setSearchValue(event.target.value)}
              />
              <button className="button button-secondary search-button" type="submit">Buscar</button>
            </form>
            <label className="sr-only" htmlFor="assistance-status">Filtrar por estado</label>
            <select
              id="assistance-status"
              className="filter-select"
              value={status}
              onChange={(event) => setStatus(event.target.value)}
            >
              <option value="">Todos los estados</option>
              <option value="Pendiente">Pendiente</option>
              <option value="EnProceso">En proceso</option>
              <option value="Resuelta">Resuelta</option>
            </select>
          </div>
        </div>

        {isLoading ? (
          <LoadingState label="Cargando solicitudes de asistencia..." />
        ) : error ? (
          <ErrorState message={error} onRetry={() => {
            setError("");
            setIsLoading(true);
            setReloadKey((key) => key + 1);
          }} />
        ) : filteredRequests.length === 0 ? (
          <EmptyState
            title={requests.length ? "No hay coincidencias" : "No hay solicitudes de asistencia"}
            description={requests.length
              ? "Ajusta la búsqueda o el estado para ver otros resultados."
              : "Las solicitudes de asistencia aparecerán aquí cuando un cliente necesite ayuda."}
          />
        ) : (
          <DataTable
            columns={columns}
            rows={filteredRequests}
            getRowKey={(request) => request.id}
            onRowClick={setSelectedRequest}
          />
        )}
        {!isLoading && !error && filteredRequests.length > 0 && (
          <div className="table-footer">
            Mostrando {filteredRequests.length} de {requests.length} solicitudes
          </div>
        )}
      </section>

      {selectedRequest && (
        <aside className="detail-panel" aria-label={`Solicitud de ${selectedRequest.cliente?.nombre ?? "cliente"}`}>
          <div className="detail-panel-top">
            <span className="table-avatar detail-avatar">{selectedRequest.cliente?.nombre?.[0] ?? "C"}</span>
            <button className="icon-button" onClick={() => setSelectedRequest(null)} aria-label="Cerrar detalle">
              <Icon name="close" />
            </button>
          </div>
          <p className="eyebrow">SOLICITUD DE ASISTENCIA · #{selectedRequest.id}</p>
          <h2>{selectedRequest.cliente?.nombre ?? "Cliente"}</h2>
          <div className="detail-field">
            <span>Estado</span>
            <StatusBadge status={selectedRequest.estado} />
          </div>
          <div className="detail-field">
            <span>Fecha de solicitud</span>
            <strong>{formatDate(selectedRequest.fechaSolicitud)}</strong>
          </div>
          <div className="detail-field">
            <span>Asesor asignado</span>
            <strong>{selectedRequest.asesorNombre || "Sin asignar"}</strong>
          </div>
          <div className="detail-description">
            <span>Motivo de asistencia</span>
            <p>{selectedRequest.motivo}</p>
          </div>
          {user.rol === "Asesor" && selectedRequest.estado === "Pendiente" && !selectedRequest.asesorId && (
            <button
              className="button button-primary detail-action"
              type="button"
              onClick={() => performAction("assign")}
              disabled={isUpdating}
            >
              {isUpdating ? "Asignando..." : "Asignarme solicitud"}
            </button>
          )}
          {user.rol === "Asesor" && selectedRequest.estado === "EnProceso"
            && selectedRequest.asesorId === user.id && (
              <button
                className="button button-primary detail-action"
                type="button"
                onClick={() => performAction("resolve")}
                disabled={isUpdating}
              >
                {isUpdating ? "Actualizando..." : "Marcar como resuelta"}
              </button>
          )}
          {actionError && <p className="inline-alert detail-alert" role="alert">{actionError}</p>}
        </aside>
      )}
    </>
  );
}
