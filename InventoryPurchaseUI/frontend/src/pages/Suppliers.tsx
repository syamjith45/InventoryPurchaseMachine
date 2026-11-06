import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import api from '../services/api'
import type { Supplier } from '../types/api'

const emptyForm = {
  name: '',
  contactPerson: '',
  phone: '',
  email: '',
  address: '',
}

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const response = (error as { response?: { data?: unknown } }).response
    if (typeof response?.data === 'string') return response.data
    if (typeof response?.data === 'object' && response.data !== null && 'message' in response.data) {
      return String((response.data as { message: unknown }).message)
    }
  }
  return 'The request could not be completed.'
}

function Suppliers() {
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [form, setForm] = useState(emptyForm)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  const loadSuppliers = useCallback(async () => {
    try {
      setLoading(true)
      setError('')
      const response = await api.get<Supplier[]>('/suppliers')
      setSuppliers(response.data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadSuppliers()
  }, [loadSuppliers])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    try {
      setSaving(true)
      setError('')
      setSuccess('')
      await api.post('/suppliers', form)
      setForm(emptyForm)
      setSuccess('Supplier added successfully.')
      await loadSuppliers()
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setSaving(false)
    }
  }

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="eyebrow">Directory</p>
          <h1>Suppliers</h1>
          <p className="page-description">Manage the suppliers used for purchase orders.</p>
        </div>
      </div>
      {error && <p className="error-message" role="alert">{error}</p>}
      {success && <p className="success-message" role="status">{success}</p>}
      <section className="panel form-panel">
        <div className="panel-heading"><div><p className="eyebrow">Add supplier</p><h2>New supplier</h2></div></div>
        <form className="entity-form" onSubmit={handleSubmit}>
          <label>Name<input required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} /></label>
          <label>Contact person<input value={form.contactPerson} onChange={(event) => setForm({ ...form, contactPerson: event.target.value })} /></label>
          <label>Phone<input value={form.phone} onChange={(event) => setForm({ ...form, phone: event.target.value })} /></label>
          <label>Email<input required type="email" value={form.email} onChange={(event) => setForm({ ...form, email: event.target.value })} /></label>
          <label className="wide-field">Address<textarea value={form.address} onChange={(event) => setForm({ ...form, address: event.target.value })} /></label>
          <div className="form-actions"><button className="primary-button" disabled={saving} type="submit">{saving ? 'Saving...' : 'Add supplier'}</button></div>
        </form>
      </section>
      <section className="panel">
        <div className="panel-heading"><div><p className="eyebrow">Supplier list</p><h2>Suppliers</h2></div></div>
        {loading ? <p className="empty-state">Loading suppliers...</p> : suppliers.length === 0 ? <p className="empty-state">No suppliers found.</p> : (
          <div className="table-wrapper"><table><thead><tr><th>Name</th><th>Contact</th><th>Phone</th><th>Email</th><th>Address</th><th>Status</th></tr></thead>
            <tbody>{suppliers.map((supplier) => <tr key={supplier.id}><td>{supplier.name}</td><td>{supplier.contactPerson || '-'}</td><td>{supplier.phone || '-'}</td><td>{supplier.email}</td><td>{supplier.address || '-'}</td><td><span className={`status-badge ${supplier.isActive ? 'active' : 'inactive'}`}>{supplier.isActive ? 'Active' : 'Inactive'}</span></td></tr>)}</tbody>
          </table></div>
        )}
      </section>
    </section>
  )
}

export default Suppliers
