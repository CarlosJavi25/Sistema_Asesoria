import { useCallback, useEffect, useMemo, useState } from 'react'
import { AuthContext } from './authContext'
import { apiRequest } from '../services/api'

const SESSION_KEY = 'sanalia.session'

function readSession() {
  try {
    const value = sessionStorage.getItem(SESSION_KEY)
    const session = value ? JSON.parse(value) : null
    if (session?.expiresAt && new Date(session.expiresAt).getTime() <= Date.now()) {
      sessionStorage.removeItem(SESSION_KEY)
      return null
    }
    return session
  } catch {
    sessionStorage.removeItem(SESSION_KEY)
    return null
  }
}

export function AuthProvider({ children }) {
  const [session, setSession] = useState(readSession)
  const [isLoading, setIsLoading] = useState(() => {
    const stored = readSession()
    return Boolean(stored?.accessToken && new Date(stored.expiresAt).getTime() > Date.now())
  })

  const logout = useCallback(() => {
    sessionStorage.removeItem(SESSION_KEY)
    setSession(null)
    setIsLoading(false)
  }, [])

  useEffect(() => {
    window.addEventListener('sanalia:unauthorized', logout)
    return () => window.removeEventListener('sanalia:unauthorized', logout)
  }, [logout])

  useEffect(() => {
    if (!session?.accessToken) return

    let isMounted = true
    apiRequest('/api/auth/me')
      .then((user) => {
        if (isMounted) setSession((current) => ({ ...current, user }))
      })
      .catch(() => {
        if (isMounted) logout()
      })
      .finally(() => {
        if (isMounted) setIsLoading(false)
      })

    return () => {
      isMounted = false
    }
  }, [session?.accessToken, session?.expiresAt, logout])

  const login = useCallback(async (email, password) => {
    const response = await apiRequest('/api/auth/login/asesor', {
      method: 'POST',
      body: { correo: email, contraseña: password },
      authenticated: false,
    })

    if (!response.esAsesor || !response.rol) {
      throw new Error('Esta cuenta no tiene acceso al espacio de asesores.')
    }

    const nextSession = {
      accessToken: response.accessToken,
      expiresAt: response.expiresAt,
      user: {
        id: response.id,
        nombre: response.nombre,
        correo: response.correo,
        esAsesor: response.esAsesor,
        rol: response.rol,
      },
    }
    sessionStorage.setItem(SESSION_KEY, JSON.stringify(nextSession))
    setSession(nextSession)
  }, [])

  const value = useMemo(() => ({
    user: session?.user ?? null,
    isAuthenticated: Boolean(session?.accessToken && session?.user),
    isLoading,
    login,
    logout,
  }), [session, isLoading, login, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
