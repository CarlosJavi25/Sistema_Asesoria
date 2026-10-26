import { useEffect, useState } from "react";
import { customerService } from "../services/api";
import { DataTable } from "../components/ui/DataTable";
import {
  EmptyState,
  ErrorState,
  LoadingState,
} from "../components/ui/Feedback";
import { Icon } from "../components/ui/Icon";
import { PageHeader } from "../components/ui/PageHeader";

function initials(name) {
  return name
    .split(" ")
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
}

export function CustomersPage() {
  const [result, setResult] = useState({ key: null, data: [], error: "" });
  const [searchValue, setSearchValue] = useState("");
  const [search, setSearch] = useState("");
  const [selectedCustomer, setSelectedCustomer] = useState(null);
  const [reloadKey, setReloadKey] = useState(0);
  const requestKey = `${search}\u0000${reloadKey}`;
  const isLoading = result.key !== requestKey;
  const customers = result.data;
  const error = result.error;

  useEffect(() => {
    const controller = new AbortController();
    customerService
      .getAll(search, controller.signal)
      .then((data) => setResult({ key: requestKey, data, error: "" }))
      .catch((requestError) => {
        if (requestError.name !== "AbortError") {
          setResult({ key: requestKey, data: [], error: requestError.message });
        }
      });
    return () => controller.abort();
  }, [search, reloadKey, requestKey]);

  const columns = [
    {
      key: "nombre",
      label: "Cliente",
      render: (customer) => (
        <span className="table-person">
          <span className="table-avatar">{initials(customer.nombre)}</span>
          <strong>{customer.nombre}</strong>
        </span>
      ),
    },
    {
      key: "correo",
      label: "Correo electrónico",
      render: (customer) => customer.correo || "—",
    },
    {
      key: "telefono",
      label: "Teléfono",
      render: (customer) => customer.telefono || "—",
    },
    {
      key: "id",
      label: "Referencia",
      render: (customer) => (
        <span className="reference-id">
          CL-{String(customer.id).padStart(4, "0")}
        </span>
      ),
    },
  ];

  function submitSearch(event) {
    event.preventDefault();
    setSearch(searchValue.trim());
  }

  return (
    <>
      <PageHeader
        eyebrow="RELACIONES QUE IMPORTAN"
        title="Clientes"
        description="Consulta y encuentra rápidamente a las personas que confían en Sanalia."
      />
      <section className="content-card directory-card">
        <div className="directory-toolbar">
          <div className="directory-count">
            <strong>{isLoading ? "—" : customers.length}</strong>
            <span>clientes en el directorio</span>
          </div>
          <form className="search-form" onSubmit={submitSearch} role="search">
            <Icon name="search" size={18} />
            <label className="sr-only" htmlFor="customer-search">
              Buscar clientes
            </label>
            <input
              id="customer-search"
              type="search"
              placeholder="Buscar por nombre, correo o teléfono"
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
        </div>

        {isLoading ? (
          <LoadingState label="Cargando clientes..." />
        ) : error ? (
          <ErrorState
            message={error}
            onRetry={() => setReloadKey((key) => key + 1)}
          />
        ) : customers.length === 0 ? (
          <EmptyState
            title={
              search
                ? "No encontramos resultados"
                : "Aún no hay clientes registrados"
            }
            description={
              search
                ? "Prueba con otro nombre, correo o número de teléfono."
                : "Cuando se registren clientes, aparecerán aquí."
            }
          />
        ) : (
          <DataTable
            columns={columns}
            rows={customers}
            getRowKey={(customer) => customer.id}
            onRowClick={setSelectedCustomer}
          />
        )}
        {!isLoading && !error && customers.length > 0 && (
          <div className="table-footer">
            {customers.length} {customers.length === 1 ? "cliente" : "clientes"}
          </div>
        )}
      </section>

      {selectedCustomer && (
        <aside
          className="detail-panel"
          aria-label={`Información de ${selectedCustomer.nombre}`}
        >
          <div className="detail-panel-top">
            <span className="table-avatar detail-avatar">
              {initials(selectedCustomer.nombre)}
            </span>
            <button
              className="icon-button"
              onClick={() => setSelectedCustomer(null)}
              aria-label="Cerrar detalle"
            >
              <Icon name="close" />
            </button>
          </div>
          <p className="eyebrow">PERFIL DEL CLIENTE</p>
          <h2>{selectedCustomer.nombre}</h2>
          <div className="detail-field">
            <span>Correo electrónico</span>
            <a href={`mailto:${selectedCustomer.correo}`}>
              {selectedCustomer.correo}
            </a>
          </div>
          <div className="detail-field">
            <span>Teléfono</span>
            <a href={`tel:${selectedCustomer.telefono}`}>
              {selectedCustomer.telefono}
            </a>
          </div>
          <div className="detail-field">
            <span>Referencia</span>
            <strong>CL-{String(selectedCustomer.id).padStart(4, "0")}</strong>
          </div>
        </aside>
      )}
    </>
  );
}
