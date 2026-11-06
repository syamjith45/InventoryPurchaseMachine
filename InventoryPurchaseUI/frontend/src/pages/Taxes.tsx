import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import api from '../services/api'
import type { Tax } from '../types/api'

const emptyForm = { name: '', percentage: '', isCompound: false }

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const data = (error as { response?: { data?: unknown } }).response?.data
    if (typeof data === 'string') return data
    if (typeof data === 'object' && data !== null && 'message' in data) return String((data as { message: unknown }).message)
  }
  return 'The request could not be completed.'
}

function Taxes() {
  const [taxes, setTaxes] = useState<Tax[]>([])
  const [form, setForm] = useState(emptyForm)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const loadTaxes = useCallback(async () => {
    try {
      setLoading(true)
      setError('')
      const response = await api.get<Tax[]>('/taxes')
      setTaxes(response.data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadTaxes()
  }, [loadTaxes])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const percentage = Number(form.percentage)
    if (!form.name.trim()) {
      setError('Tax name is required.')
      return
    }
    if (!Number.isFinite(percentage) || percentage < 0) {
      setError('Tax percentage must be zero or greater.')
      return
    }
    try {
      setSaving(true)
      setError('')
      setSuccess('')
      await api.post('/taxes', { name: form.name, percentage, isCompound: form.isCompound })
      setForm(emptyForm)
      setSuccess('Tax added successfully.')
      await loadTaxes()
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setSaving(false)
    }
  }

  return (
    <section>
      <div className="page-heading"><div><p className="eyebrow">Configuration</p><h1>Taxes</h1><p className="page-description">Manage taxes that can be assigned to products.</p></div></div>
      {error && <p className="error-message" role="alert">{error}</p>}
      {success && <p className="success-message" role="status">{success}</p>}
      <section className="panel form-panel">
        <div className="panel-heading"><div><p className="eyebrow">Add tax</p><h2>New tax</h2></div></div>
        <form className="entity-form tax-form" onSubmit={handleSubmit}>
          <label>Name<input required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} /></label>
          <label>Percentage<input required min="0" step="0.01" type="number" value={form.percentage} onChange={(event) => setForm({ ...form, percentage: event.target.value })} /></label>
          <label className="checkbox-field"><input type="checkbox" checked={form.isCompound} onChange={(event) => setForm({ ...form, isCompound: event.target.checked })} /> Compound tax</label>
          <div className="form-actions"><button className="primary-button" disabled={saving} type="submit">{saving ? 'Saving...' : 'Add tax'}</button></div>
        </form>
      </section>
      <section className="panel">
        <div className="panel-heading"><div><p className="eyebrow">Tax list</p><h2>Taxes</h2></div></div>
        {loading ? <p className="empty-state">Loading taxes...</p> : taxes.length === 0 ? <p className="empty-state">No taxes found.</p> : (
          <div className="table-wrapper"><table><thead><tr><th>Name</th><th>Percentage</th><th>Type</th><th>Status</th></tr></thead>
            <tbody>{taxes.map((tax) => <tr key={tax.id}><td>{tax.name}</td><td>{tax.percentage.toFixed(2)}%</td><td>{tax.isCompound ? 'Compound' : 'Simple'}</td><td><span className={`status-badge ${tax.isActive ? 'active' : 'inactive'}`}>{tax.isActive ? 'Active' : 'Inactive'}</span></td></tr>)}</tbody>
          </table></div>
        )}
      </section>
    </section>
  )
}

export default Taxes
