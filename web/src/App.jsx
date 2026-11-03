import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./context/useAuth";
import { ProtectedLayout } from "./components/layout/ProtectedLayout";
import { LoginPage } from "./pages/auth/LoginPage";
import { CustomersPage } from "./pages/CustomersPage";
import { DashboardPage } from "./pages/DashboardPage";
import { EnquiriesPage } from "./pages/EnquiriesPage";
import { ComparatorPage } from "./pages/ComparatorPage";
import { ContributionsPage } from "./pages/ContributionsPage";
import { AsistencePage } from "./pages/AsistencePage";
import { InsurersPage } from "./pages/InsurersPage";
import { LoadingState } from "./components/ui/Feedback";

function HomeRoute() {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading)
    return <LoadingState label="Preparando tu espacio de trabajo..." />;
  return <Navigate to={isAuthenticated ? "/app" : "/login"} replace />;
}

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomeRoute />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/app" element={<ProtectedLayout />}>
        <Route index element={<DashboardPage />} />
        <Route path="clientes" element={<CustomersPage />} />
        <Route path="solicitudes" element={<EnquiriesPage />} />
        <Route path="comparador" element={<ComparatorPage />} />
        <Route path="aportes" element={<ContributionsPage />} />
        <Route path="asistencia" element={<AsistencePage />} />
        <Route
          path="aseguradoras"
          element={(
            <AdminRoute>
              <InsurersPage />
            </AdminRoute>
          )}
        />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

function AdminRoute({ children }) {
  const { user } = useAuth();
  return user?.rol === "Administrador" ? children : <Navigate to="/app" replace />;
}
