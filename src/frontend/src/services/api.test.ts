import { describe, expect, it, vi } from 'vitest'
import { createConversion } from './api'

describe('api', () => {
  it('POSTs to the expected endpoint', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => {
      return {
        ok: true,
        status: 200,
        async json() {
          return {
            auditId: 'a1',
            amount: 10,
            fromCurrency: 'USD',
            toCurrency: 'EUR',
            rate: 0.9,
            convertedAmount: 9,
            providerDate: '2026-08-01',
            executedAtUtc: '2026-08-01T00:00:00.000Z'
          }
        },
        async text() {
          return ''
        }
      } as any
    }))

    const res = await createConversion('', {
      amount: 10,
      fromCurrency: 'USD',
      toCurrency: 'EUR'
    })

    expect(res.auditId).toBe('a1')
    expect((globalThis.fetch as any).mock.calls[0][0]).toBe('/api/conversions')
  })
})
