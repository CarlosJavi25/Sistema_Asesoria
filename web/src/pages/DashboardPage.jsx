import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  assistanceService,
  customerService,
  enquiryService,
} from "../services/api";
import { ErrorState, LoadingState } from "../components/ui/Feedback";
import { Icon } from "../components/ui/Icon";
import { PageHeader } from "../components/ui/PageHeader";
import { StatusBadge } from "../components/ui/StatusBadge";

function formatDate(value) {
  return new Intl.DateTimeFormat("es-DO", {
    day: "numeric",
    month: "short",
  }).format(new Date(value));
}

export function DashboardPage() {
  const [result, setResult] = useState({ key: null, data: null, error: "" });
  const [reloadKey, setReloadKey] = useState(0);
  const isLoading = result.key !== reloadKey;
  const data = result.data;
  const error = result.error;

  useEffect(() => {
    const controller = new AbortController();
    Promise.all([
      customerService.getAll("", controller.signal),
      enquiryService.getPending(controller.signal),
      assistanceService.getOpen(controller.signal),
    ])
      .then(([customers, enquiries, assistance]) =>
        setResult({
          key: reloadKey,
          data: { customers, enquiries, assistance },
          error: "",
        }),
      )
      .catch((requestError) => {
        if (requestError.name !== "AbortError") {
          setResult({
            key: reloadKey,
            data: null,
            error: requestError.message,
          });
        }
      });
    return () => controller.abort();
  }, [reloadKey]);

  if (isLoading)
    return <LoadingState label="Cargando el resumen de tu actividad..." />;
  if (error)
    return (
      <ErrorState
        message={error}
        onRetry={() => setReloadKey((key) => key + 1)}
      />
    );

  const stats = [
    {
      label: "Clientes registrados",
      value: data.customers.length,
      note: "En el directorio",
      icon: "people",
      to: "/app/clientes",
    },
    {
      label: "Solicitudes pendientes",
      value: data.enquiries.length,
      note: "Requieren seguimiento",
      icon: "message",
      to: "/app/solicitudes",
    },
    {
      label: "Asistencias abiertas",
      value: data.assistance.length,
      note: "Casos por atender",
      icon: "shield",
      to: "/app/solicitudes",
    },
  ];
  const recentEnquiries = [...data.enquiries]
    .sort(
      (first, second) =>
        new Date(second.fechaSolicitud) - new Date(first.fechaSolicitud),
    )
    .slice(0, 5);

  return (
    <>
      <PageHeader
        eyebrow="TU JORNADA, MÁS CLARA"
        title="Resumen"
        description="Aquí tienes una vista rápida de lo que sucede en tu espacio de trabajo."
      />

      <section className="stats-grid" aria-label="Indicadores principales">
        {stats.map((stat) => (
          <Link className="stat-card" key={stat.label} to={stat.to}>
            <span className="stat-icon">
              <Icon name={stat.icon} />
            </span>
            <span className="stat-label">{stat.label}</span>
            <strong className="stat-value">{stat.value}</strong>
            <span className="stat-note">{stat.note}</span>
          </Link>
        ))}
      </section>

      <section className="content-card dashboard-activity">
        <div className="card-heading">
          <div>
            <p className="eyebrow">SEGUIMIENTO</p>
            <h2>Solicitudes recientes</h2>
          </div>
          <Link className="text-link" to="/app/solicitudes">
            Ver solicitudes <Icon name="arrow" size={16} />
          </Link>
        </div>
        {recentEnquiries.length === 0 ? (
          <p className="subtle-empty">
            No hay solicitudes pendientes por el momento.
          </p>
        ) : (
          <div className="activity-list">
            {recentEnquiries.map((item) => (
              <Link
                className="activity-row"
                to="/app/solicitudes"
                key={item.id}
              >
                <span className="activity-avatar">
                  {item.cliente?.nombre?.[0] ?? "C"}
                </span>
                <span className="activity-main">
                  <strong>{item.cliente?.nombre ?? "Cliente"}</strong>
                  <small>{item.tipoSeguro}</small>
                </span>
                <StatusBadge status={item.estado} />
                <span className="activity-date">
                  {formatDate(item.fechaSolicitud)}
                </span>
                <Icon name="arrow" size={17} className="activity-arrow" />
              </Link>
            ))}
          </div>
        )}
      </section>

      <section className="welcome-strip">
        <span className="welcome-strip-icon">
          <Icon name="shield" size={24} />
        </span>
        <div>
          <strong>Una buena asesoría empieza escuchando.</strong>
          <p>
            Revisa las solicitudes nuevas y acompaña a cada cliente en su
            próxima decisión.
          </p>
        </div>
        <Link
          className="welcome-strip-link"
          to="/app/solicitudes"
          aria-label="Ver solicitudes"
        >
          <Icon name="arrow" />
        </Link>
      </section>
    </>
  );
}
