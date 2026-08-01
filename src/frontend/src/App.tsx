import { useMemo, useState } from 'react'
import { createConversion, getConversionById } from './services/api'
import type { ConversionRequest, ConversionResponse } from './services/types'

function formatError(err: unknown): string {
  if (typeof err === 'string') return err
  if (err instanceof Error) return err.message
  return 'Something went wrong'
}

export default function App({ apiBaseUrl }: { apiBaseUrl: string }) {
  const [amount, setAmount] = useState('')
  const [fromCurrency, setFromCurrency] = useState('USD')
  const [toCurrency, setToCurrency] = useState('EUR')

  const [loading, setLoading] = useState(false)
  const [conversionError, setConversionError] = useState<string | null>(null)
  const [conversionResult, setConversionResult] = useState<ConversionResponse | null>(null)

  const [lookupAuditId, setLookupAuditId] = useState('')
  const [lookupLoading, setLookupLoading] = useState(false)
  const [lookupError, setLookupError] = useState<string | null>(null)
  const [lookupResult, setLookupResult] = useState<ConversionResponse | null>(null)

  const fromUpper = useMemo(() => fromCurrency.trim().toUpperCase(), [fromCurrency])
  const toUpper = useMemo(() => toCurrency.trim().toUpperCase(), [toCurrency])

  async function onConvertSubmit(e: React.FormEvent) {
    e.preventDefault()
    setConversionError(null)
    setConversionResult(null)

    const parsedAmount = Number(amount)
    if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
      setConversionError('amount must be a number greater than 0')
      return
    }

    const request: ConversionRequest = {
      amount: parsedAmount,
      fromCurrency: fromUpper,
      toCurrency: toUpper
    }

    setLoading(true)
    try {
      const res = await createConversion(apiBaseUrl, request)
      setConversionResult(res)
    } catch (err) {
      setConversionError(formatError(err))
    } finally {
      setLoading(false)
    }
  }

  async function onLookupSubmit(e: React.FormEvent) {
    e.preventDefault()
    setLookupError(null)
    setLookupResult(null)
    if (!lookupAuditId.trim()) {
      setLookupError('auditId is required')
      return
    }

    setLookupLoading(true)
    try {
      const res = await getConversionById(apiBaseUrl, lookupAuditId.trim())
      setLookupResult(res)
    } catch (err) {
      setLookupError(formatError(err))
    } finally {
      setLookupLoading(false)
    }
  }

  return (
    <div className="page">
      <h1>Real-Time Currency Conversion</h1>

      <section className="card">
        <h2>New Conversion</h2>
        <form onSubmit={onConvertSubmit} className="form">
          <label>
            Amount
            <input value={amount} onChange={(e) => setAmount(e.target.value)} inputMode="decimal" />
          </label>
          <div className="row">
            <label>
              From
              <input value={fromCurrency} onChange={(e) => setFromCurrency(e.target.value)} />
            </label>
            <label>
              To
              <input value={toCurrency} onChange={(e) => setToCurrency(e.target.value)} />
            </label>
          </div>
          <button disabled={loading} type="submit">
            {loading ? 'Converting...' : 'Convert'}
          </button>
        </form>

        {conversionError && <div className="error">{conversionError}</div>}

        {conversionResult && (
          <div className="result">
            <div className="kv">
              <div>Converted</div>
              <div>
                {conversionResult.convertedAmount} {conversionResult.toCurrency}
              </div>
            </div>
            <div className="kv">
              <div>Rate</div>
              <div>{conversionResult.rate}</div>
            </div>
            <div className="kv">
              <div>Provider date</div>
              <div>{conversionResult.providerDate}</div>
            </div>
            <div className="kv">
              <div>Executed at (UTC)</div>
              <div className="mono">{conversionResult.executedAtUtc}</div>
            </div>
            <div className="kv">
              <div>Audit ID</div>
              <div className="mono">{conversionResult.auditId}</div>
            </div>
          </div>
        )}
      </section>

      <section className="card">
        <h2>Audit Lookup</h2>
        <form onSubmit={onLookupSubmit} className="form">
          <label>
            Audit ID
            <input value={lookupAuditId} onChange={(e) => setLookupAuditId(e.target.value)} />
          </label>
          <button disabled={lookupLoading} type="submit">
            {lookupLoading ? 'Loading...' : 'Lookup'}
          </button>
        </form>

        {lookupError && <div className="error">{lookupError}</div>}

        {lookupResult && (
          <div className="result">
            <div className="kv">
              <div>Converted</div>
              <div>
                {lookupResult.convertedAmount} {lookupResult.toCurrency}
              </div>
            </div>
            <div className="kv">
              <div>Rate</div>
              <div>{lookupResult.rate}</div>
            </div>
            <div className="kv">
              <div>Provider date</div>
              <div>{lookupResult.providerDate}</div>
            </div>
            <div className="kv">
              <div>Executed at (UTC)</div>
              <div className="mono">{lookupResult.executedAtUtc}</div>
            </div>
          </div>
        )}
      </section>
    </div>
  )
}
