const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? ''

export class ApiError extends Error {
  constructor(message, status) {
    super(message)
    this.name = 'ApiError'
    this.status = status
  }
}

export async function apiRequest(path, { method = 'GET', body, signal, authenticated = true } = {}) {
  const headers = { Accept: 'application/json' }
  if (body !== undefined) headers['Content-Type'] = 'application/json'

  if (authenticated) {
    try {
      const session = JSON.parse(sessionStorage.getItem('sanalia.session') ?? 'null')
      if (session?.accessToken) headers.Authorization = `Bearer ${session.accessToken}`
    } catch {
      sessionStorage.removeItem('sanalia.session')
    }
  }

  let response
  try {
    response = await fetch(`${apiBaseUrl}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
      signal,
    })
  } catch (error) {
    if (error.name === 'AbortError') throw error
    throw new ApiError('No fue posible conectar con el servidor. Inténtalo de nuevo.', 0)
  }

  if (!response.ok) {
    let message = `Ocurrió un error al procesar la solicitud (${response.status}).`
    try {
      const problem = await response.json()
      message = problem.detail || problem.title || message
      if (problem.errors) {
        const validationErrors = Object.values(problem.errors).flat()
        if (validationErrors.length) message = validationErrors.join(' ')
      }
      if (response.status === 401) {
        message = path.includes('/auth/login/')
          ? 'El correo electrónico o la contraseña no son correctos.'
          : 'Tu sesión venció. Inicia sesión nuevamente.'
      }
      if (response.status === 403) message = 'No tienes permiso para realizar esta acción.'
    } catch {
      if (response.status === 401) {
        message = path.includes('/auth/login/')
          ? 'El correo electrónico o la contraseña no son correctos.'
          : 'Tu sesión venció. Inicia sesión nuevamente.'
      }
    }
    if (response.status === 401 && !path.includes('/auth/login/') && typeof window !== 'undefined') {
      window.dispatchEvent(new Event('sanalia:unauthorized'))
    }
    throw new ApiError(message, response.status)
  }

  if (response.status === 204) return null
  return response.json()
}

export const customerService = {
  getAll: (search, signal) => {
    const query = search ? `?busqueda=${encodeURIComponent(search)}` : ''
    return apiRequest(`/api/clientes${query}`, { signal })
  },
}

export const enquiryService = {
  getPending: (signal) => apiRequest('/api/solicitudes-asesoria/pendientes', { signal }),
}

export const assistanceService = {
  getOpen: (signal) => apiRequest('/api/solicitudes-asistencia/abiertas', { signal }),
}
