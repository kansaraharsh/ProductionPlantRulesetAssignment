import { useState } from 'react'

type Evaluation = {
  matched: boolean
  productionPlant?: string
  matchedRuleset?: string
  matchedRule?: string
  reason: string
}

const sampleOrder = {
  orderId: '1245101',
  publisherNumber: '99999',
  publisherName: 'BookWorld Ltd',
  orderMethod: 'POD',
  shipments: [{ shipTo: { isoCountry: 'US' } }],
  items: [{
    sku: 'PB-001',
    printQuantity: 10,
    components: [
      { code: 'Cover', attributes: { BindTypeCode: 'PB' } },
      { code: 'Content', attributes: { BindTypeCode: 'PB' } }
    ]
  }]
}

const API = import.meta.env.VITE_API_BASE_URL ?? 'https://api-production-plant-d7cjaedzfacucpbn.centralindia-01.azurewebsites.net'

export default function App() {
  const [json, setJson] = useState(JSON.stringify(sampleOrder, null, 2))
  const [result, setResult] = useState<Evaluation | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function evaluate() {
    setLoading(true)
    setError('')
    setResult(null)

    try {
      const parsed = JSON.parse(json)
      const response = await fetch(`${API}/api/evaluate`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(parsed)
      })

      const body = await response.json()
      if (!response.ok) {
        setError(body.detail ?? body.message ?? 'Evaluation failed.')
        return
      }

      setResult(body)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unable to evaluate order.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page">
      <header>
        <div>
          <div className="eyebrow">RULES ENGINE</div>
          <h1>Production Plant Evaluator</h1>
          <p>Submit an order JSON and see the exact ruleset and rule that determine the production plant.</p>
        </div>
        <div className="badge">.NET 8 + SQL Server + React</div>
      </header>

      <main className="grid">
        <section className="card">
          <div className="card-title">
            <h2>Order JSON</h2>
            <button className="secondary" onClick={() => setJson(JSON.stringify(sampleOrder, null, 2))}>
              Load sample
            </button>
          </div>

          <textarea value={json} onChange={e => setJson(e.target.value)} spellCheck={false} />

          <button className="primary" onClick={evaluate} disabled={loading}>
            {loading ? 'Evaluating…' : 'Evaluate Order'}
          </button>

          {error && <div className="error">{error}</div>}
        </section>

        <section className="card result-card">
          <div className="card-title">
            <h2>Decision</h2>
            {result && <span className={result.matched ? 'status ok' : 'status fail'}>
              {result.matched ? 'MATCHED' : 'NO MATCH'}
            </span>}
          </div>

          {!result && <div className="empty">Evaluation results will appear here.</div>}

          {result && (
            <>
              <div className="plant">{result.productionPlant ?? '—'}</div>
              <div className="details">
                <div><span>Ruleset</span><strong>{result.matchedRuleset ?? '—'}</strong></div>
                <div><span>Rule</span><strong>{result.matchedRule ?? '—'}</strong></div>
                <div><span>Reason</span><strong>{result.reason}</strong></div>
              </div>
            </>
          )}
        </section>
      </main>

      <footer>
        <span>Configurable DB-backed evaluation</span>
        <span>•</span>
        <span>Auditable decisions</span>
        <span>•</span>
        <span>Azure-ready</span>
      </footer>
    </div>
  )
}
