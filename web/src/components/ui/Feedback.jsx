export function LoadingState({ label = 'Cargando...' }) {
  return (
    <div className="feedback-state" role="status">
      <span className="spinner" aria-hidden="true" />
      <span>{label}</span>
    </div>
  )
}

export function ErrorState({ message, onRetry }) {
  return (
    <div className="feedback-card feedback-error" role="alert">
      <p>{message}</p>
      {onRetry && <button className="button button-secondary" onClick={onRetry}>Reintentar</button>}
    </div>
  )
}

export function EmptyState({ title, description }) {
  return (
    <div className="empty-state">
      <span className="empty-mark" aria-hidden="true">S</span>
      <h3>{title}</h3>
      <p>{description}</p>
    </div>
  )
}
