import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import { LoadingState } from "../../components/ui/Feedback";
import { Icon } from "../../components/ui/Icon";

export function LoginPage() {
  const { isAuthenticated, isLoading, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (isLoading) return <LoadingState label="Verificando tu sesión..." />;
  if (isAuthenticated) return <Navigate to="/app" replace />;

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);
    try {
      await login(email.trim(), password);
      navigate(location.state?.from?.pathname ?? "/app", { replace: true });
    } catch (requestError) {
      setError(requestError.message);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <main className="login-page">
      <section className="login-brand-panel">
        <div className="login-brand">
          <span className="brand-shield">
            <Icon name="shield" size={30} />
          </span>
          <span className="brand-copy">
            <strong>SANALIA</strong>
            <small>&amp; ASOCIADOS, S.R.L.</small>
          </span>
        </div>
        <div className="login-brand-message">
          <p className="eyebrow eyebrow-gold">CORREDORES DE SEGUROS</p>
          <h1>Protegemos lo que más importa.</h1>
          <p>
            Acompañamos a cada persona y empresa con soluciones hechas a su
            medida.
          </p>
        </div>
        <p className="login-brand-footer">
          Siéntete más que seguro. Somos soluciones.
        </p>
      </section>

      <section className="login-form-panel">
        <div className="login-form-wrap">
          <div className="login-form-heading">
            <p className="eyebrow">PORTAL DE ASESORES</p>
            <h2>Te damos la bienvenida</h2>
            <p>Inicia sesión para continuar con tu jornada.</p>
          </div>
          <form className="login-form" onSubmit={handleSubmit}>
            <label htmlFor="email">Correo electrónico</label>
            <input
              autoComplete="username"
              id="email"
              name="email"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="nombre@sanalia.com"
              required
            />
            <label htmlFor="password">Contraseña</label>
            <input
              autoComplete="current-password"
              id="password"
              name="password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Ingresa tu contraseña"
              required
            />
            {error && (
              <p className="form-error" role="alert">
                {error}
              </p>
            )}
            <button
              className="button button-primary login-submit"
              type="submit"
              disabled={isSubmitting}
            >
              {isSubmitting ? "Ingresando..." : "Iniciar sesión"}
              {!isSubmitting && <Icon name="arrow" size={18} />}
            </button>
          </form>
          <p className="login-support">
            ¿Necesitas ayuda?{" "}
            <a href="mailto:info@sanaliayasociados.com">Contáctanos</a>
          </p>
        </div>
        <p className="login-copyright">
          © {new Date().getFullYear()} Sanalia &amp; Asociados, S.R.L.
        </p>
      </section>
    </main>
  );
}
