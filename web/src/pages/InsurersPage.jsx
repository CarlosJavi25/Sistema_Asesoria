import { useEffect, useState } from "react";
import { insurerService } from "../services/api";
import { DataTable } from "../components/ui/DataTable";
import { EmptyState, ErrorState, LoadingState } from "../components/ui/Feedback";
import { Icon } from "../components/ui/Icon";
import { PageHeader } from "../components/ui/PageHeader";

const emptyForm = { nombre: "", telefono: "", correoContacto: "" };

export function InsurersPage() {
  const [insurers, setInsurers] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    insurerService.getAll(controller.signal)
      .then(setInsurers)
      .catch((requestError) => {
        if (requestError.name !== "AbortError") setError(requestError.message);
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsLoading(false);
      });
    return () => controller.abort();
  }, [reloadKey]);

  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
    setError("");
  }

  function editInsurer(insurer) {
    setEditingId(insurer.id);
    setForm({
      nombre: insurer.nombre,
      telefono: insurer.telefono ?? "",
      correoContacto: insurer.correoContacto ?? "",
    });
    setError("");
    setSuccess("");
  }

  async function saveInsurer(event) {
    event.preventDefault();
    setError("");
    setSuccess("");
    setIsSaving(true);
    const payload = {
      nombre: form.nombre.trim(),
      telefono: form.telefono.trim() || null,
      correoContacto: form.correoContacto.trim() || null,
    };
    try {
      const savedInsurer = editingId
        ? await insurerService.update(editingId, payload)
        : await insurerService.create(payload);
      setInsurers((current) => editingId
        ? current.map((insurer) => insurer.id === editingId ? savedInsurer : insurer)
        : [...current, savedInsurer].sort((a, b) => a.nombre.localeCompare(b.nombre, "es")));
      setSuccess(editingId ? "Aseguradora actualizada correctamente." : "Aseguradora creada correctamente.");
      setForm(emptyForm);
      setEditingId(null);
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteInsurer(insurer) {
    if (!window.confirm(`¿Seguro que deseas eliminar a ${insurer.nombre}?`)) return;
    setError("");
    setSuccess("");
    try {
      await insurerService.delete(insurer.id);
      setInsurers((current) => current.filter((item) => item.id !== insurer.id));
      if (editingId === insurer.id) resetForm();
      setSuccess("Aseguradora eliminada correctamente.");
    } catch (requestError) {
      setError(requestError.message);
    }
  }

  const columns = [
    { key: "nombre", label: "Aseguradora" },
    { key: "telefono", label: "Teléfono", render: (insurer) => insurer.telefono || "—" },
    { key: "correoContacto", label: "Correo de contacto", render: (insurer) => insurer.correoContacto || "—" },
    {
      key: "actions",
      label: "Acciones",
      render: (insurer) => (
        <span className="insurer-row-actions">
          <button
            className="button button-secondary"
            type="button"
            onClick={(event) => {
              event.stopPropagation();
              editInsurer(insurer);
            }}
          >
            Editar
          </button>
          <button
            className="button insurer-delete-button"
            type="button"
            onClick={(event) => {
              event.stopPropagation();
              deleteInsurer(insurer);
            }}
          >
            Eliminar
          </button>
        </span>
      ),
    },
  ];

  return (
    <>
      <PageHeader
        eyebrow="CATÁLOGO DE ALIADOS"
        title="Aseguradoras"
        description="Administra las compañías que ofrecen productos de seguro en el catálogo."
        action={
          editingId ? (
            <button className="button button-secondary" type="button" onClick={resetForm}>
              Nueva aseguradora <Icon name="plus" size={17} />
            </button>
          ) : null
        }
      />

      {error && <div className="inline-alert" role="alert">{error}</div>}
      {success && <div className="inline-success" role="status">{success}</div>}

      <div className="insurer-management">
        <section className="content-card directory-card">
          <div className="directory-toolbar">
            <div className="directory-count">
              <strong>{isLoading ? "—" : insurers.length}</strong>
              <span>aseguradoras disponibles</span>
            </div>
          </div>
          {isLoading ? (
            <LoadingState label="Cargando aseguradoras..." />
          ) : error && insurers.length === 0 ? (
            <ErrorState message={error} onRetry={() => {
              setError("");
              setIsLoading(true);
              setReloadKey((key) => key + 1);
            }} />
          ) : insurers.length === 0 ? (
            <EmptyState
              title="Aún no hay aseguradoras"
              description="Registra la primera aseguradora para comenzar a completar el catálogo."
            />
          ) : (
            <DataTable columns={columns} rows={insurers} getRowKey={(insurer) => insurer.id} />
          )}
        </section>

        <section className="content-card insurer-form-card">
          <div className="insurer-form-heading">
            <span className="insurer-form-icon"><Icon name="building" size={20} /></span>
            <div>
              <p className="eyebrow">{editingId ? "ACTUALIZAR DATOS" : "NUEVA COMPAÑÍA"}</p>
              <h2>{editingId ? "Editar aseguradora" : "Registrar aseguradora"}</h2>
            </div>
          </div>
          <form className="insurer-form" onSubmit={saveInsurer}>
            <label htmlFor="insurer-name">Nombre de la aseguradora</label>
            <input
              id="insurer-name"
              name="nombre"
              maxLength={100}
              required
              value={form.nombre}
              onChange={(event) => setForm((current) => ({ ...current, nombre: event.target.value }))}
              placeholder="Ej. Seguros Sanalia"
            />
            <label htmlFor="insurer-phone">Teléfono de contacto</label>
            <input
              id="insurer-phone"
              name="telefono"
              maxLength={20}
              value={form.telefono}
              onChange={(event) => setForm((current) => ({ ...current, telefono: event.target.value }))}
              placeholder="809-555-0100"
            />
            <label htmlFor="insurer-email">Correo de contacto</label>
            <input
              id="insurer-email"
              name="correoContacto"
              type="email"
              maxLength={100}
              value={form.correoContacto}
              onChange={(event) => setForm((current) => ({ ...current, correoContacto: event.target.value }))}
              placeholder="contacto@aseguradora.com"
            />
            <div className="insurer-form-actions">
              {editingId && (
                <button className="button button-secondary" type="button" onClick={resetForm}>
                  Cancelar
                </button>
              )}
              <button className="button button-primary" type="submit" disabled={isSaving}>
                {isSaving ? "Guardando..." : editingId ? "Guardar cambios" : "Crear aseguradora"}
              </button>
            </div>
          </form>
        </section>
      </div>
    </>
  );
}
