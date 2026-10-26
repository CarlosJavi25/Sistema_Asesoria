const statuses = {
  Pendiente: 'warning',
  Confirmada: 'info',
  Atendida: 'success',
  Cancelada: 'neutral',
  EnProceso: 'info',
  Resuelta: 'success',
}

const labels = {
  EnProceso: 'En proceso',
}

export function StatusBadge({ status }) {
  return <span className={`status-badge status-${statuses[status] ?? 'neutral'}`}>{labels[status] ?? status}</span>
}
