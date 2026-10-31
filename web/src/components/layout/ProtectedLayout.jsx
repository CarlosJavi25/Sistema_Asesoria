import { NavLink, Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import { Icon } from "../ui/Icon";
import { LoadingState } from "../ui/Feedback";

const navigation = [
  { to: "/app", label: "Resumen", icon: "dashboard", end: true },
  { to: "/app/clientes", label: "Clientes", icon: "people" },
  { to: "/app/solicitudes", label: "Solicitudes", icon: "message" },
  { to: "/app/comparador", label: "Comparador", icon: "compare" },
  { to: "/app/aportes", label: "Aportes", icon: "quote" },
];

export function ProtectedLayout() {
  const { isAuthenticated, isLoading, user, logout } = useAuth();
  const location = useLocation();

  if (isLoading) return <LoadingState label="Verificando tu sesión..." />;
  if (!isAuthenticated)
    return <Navigate to="/login" replace state={{ from: location }} />;

  const initials = user.nombre
    .split(" ")
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <NavLink to="/app" className="brand-lockup">
          <span className="brand-shield">
            <Icon name="shield" size={25} />
          </span>
          <span className="brand-copy">
            <strong>SANALIA</strong>
            <small>&amp; ASOCIADOS, S.R.L.</small>
          </span>
        </NavLink>

        <div className="nav-section-label">ESPACIO DE TRABAJO</div>
        <nav className="primary-nav" aria-label="Navegación principal">
          {navigation.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                `nav-link${isActive ? " active" : ""}`
              }
            >
              <Icon name={item.icon} />
              <span>{item.label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="sidebar-promise">
          <span className="promise-rule" />
          <p>Siéntete más que seguro.</p>
          <span>Somos soluciones.</span>
        </div>

        <button className="sidebar-user" onClick={logout}>
          <span className="avatar">{initials}</span>
          <span className="user-copy">
            <strong>{user.nombre}</strong>
            <small>
              {user.rol === "Administrador" ? "Administrador" : "Asesor"}
            </small>
          </span>
          <Icon name="logout" size={18} />
          <span className="sr-only">Cerrar sesión</span>
        </button>
      </aside>

      <div className="app-main">
        <header className="topbar">
          <div className="breadcrumb">
            <span>Sanalia</span>
            <span className="breadcrumb-divider">/</span>
            <span>
              {navigation.find((item) => item.to === location.pathname)
                ?.label ?? "Espacio de trabajo"}
            </span>
          </div>
          <div className="topbar-actions">
            <div className="topbar-date">
              {new Intl.DateTimeFormat("es-DO", { dateStyle: "long" }).format(
                new Date(),
              )}
            </div>
            <button
              className="mobile-logout"
              onClick={logout}
              aria-label="Cerrar sesión"
            >
              <Icon name="logout" size={17} />
            </button>
          </div>
        </header>
        <main className="page-content">
          <Outlet />
        </main>
        <footer className="app-footer">
          Sanalia &amp; Asociados <span>·</span> Tu aliado en cada decisión
        </footer>
      </div>
    </div>
  );
}
