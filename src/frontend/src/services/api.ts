import type { ConversionRequest, ConversionResponse } from './types'

function joinApi(base: string, path: string): string {
  if (!base) return path
  if (base.endsWith('/')) return `${base.slice(0, -1)}${path}`
  return `${base}${path}`
}

export async function createConversion(
  apiBaseUrl: string,
  request: ConversionRequest
): Promise<ConversionResponse> {
  const url = joinApi(apiBaseUrl, '/api/conversions')
  const res = await fetch(url, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request)
  })

  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `Request failed with status ${res.status}`)
  }

  return (await res.json()) as ConversionResponse
}

export async function getConversionById(
  apiBaseUrl: string,
  auditId: string
): Promise<ConversionResponse> {
  const url = joinApi(apiBaseUrl, `/api/conversions/${encodeURIComponent(auditId)}`)
  const res = await fetch(url)
  if (!res.ok) {
    const text = await res.text()
    throw new Error(text || `Request failed with status ${res.status}`)
  }
  return (await res.json()) as ConversionResponse
}
