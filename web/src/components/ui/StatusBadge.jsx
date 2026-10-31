const statuses = {
  Pendiente: 'warning',
  Confirmada: 'info',
  Atendida: 'success',
  Cancelada: 'neutral',
  EnProceso: 'info',
  Resuelta: 'success',
  Borrador: 'neutral',
  Presentada: 'info',
  Aceptada: 'success',
  Rechazada: 'neutral',
}

const labels = {
  EnProceso: 'En proceso',
  Basico: 'Básico',
  Intermedio: 'Intermedio',
  Completo: 'Completo',
}

export function StatusBadge({ status }) {
  return <span className={`status-badge status-${statuses[status] ?? 'neutral'}`}>{labels[status] ?? status}</span>
}
