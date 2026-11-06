import { useCallback, useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import api from '../services/api'
import type { Product, ProductFormData } from '../types/product'

const emptyForm: ProductFormData = {
  name: '',
  code: '',
  category: '',
  costPrice: '',
  sellingPrice: '',
}

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const response = (error as { response?: { data?: unknown } }).response
    if (typeof response?.data === 'string') {
      return response.data
    }
  }

  return 'The request could not be completed.'
}

function Products() {
  const [products, setProducts] = useState<Product[]>([])
  const [form, setForm] = useState<ProductFormData>(emptyForm)
  const [editingId, setEditingId] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  const loadProducts = useCallback(async () => {
    try {
      setLoading(true)
      setError('')
      const response = await api.get<Product[]>('/products')
      setProducts(response.data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadProducts()
  }, [loadProducts])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    try {
      setSaving(true)
      setError('')
      const payload = {
        name: form.name,
        code: form.code,
        category: form.category,
        costPrice: Number(form.costPrice),
        sellingPrice: Number(form.sellingPrice),
      }

      if (editingId === null) {
        await api.post<Product>('/products', payload)
      } else {
        await api.put(`/products/${editingId}`, payload)
      }

      setForm(emptyForm)
      setEditingId(null)
      await loadProducts()
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setSaving(false)
    }
  }

  const handleEdit = (product: Product) => {
    setEditingId(product.id)
    setForm({
      name: product.name,
      code: product.code,
      category: product.category,
      costPrice: String(product.costPrice),
      sellingPrice: String(product.sellingPrice),
    })
  }

  const handleDelete = async (product: Product) => {
    if (!window.confirm(`Delete ${product.name}?`)) {
      return
    }

    try {
      setError('')
      await api.delete(`/products/${product.id}`)
      await loadProducts()
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }

  const cancelEdit = () => {
    setEditingId(null)
    setForm(emptyForm)
  }

  return (
    <section className="products-page">
      <div className="page-heading">
        <div>
          <p className="eyebrow">Catalog</p>
          <h1>Products</h1>
          <p className="page-description">Manage the products available for purchasing and inventory.</p>
        </div>
      </div>

      {error && <p className="error-message" role="alert">{error}</p>}

      <section className="panel product-form-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">{editingId === null ? 'Add product' : 'Edit product'}</p>
            <h2>{editingId === null ? 'New product' : 'Update product'}</h2>
          </div>
        </div>
        <form className="product-form" onSubmit={handleSubmit}>
          <label>
            Name
            <input required value={form.name} onChange={(event) => setForm({ ...form, name: event.target.value })} />
          </label>
          <label>
            Code
            <input required value={form.code} onChange={(event) => setForm({ ...form, code: event.target.value })} />
          </label>
          <label>
            Category
            <input value={form.category} onChange={(event) => setForm({ ...form, category: event.target.value })} />
          </label>
          <label>
            Cost price
            <input required min="0" step="0.01" type="number" value={form.costPrice} onChange={(event) => setForm({ ...form, costPrice: event.target.value })} />
          </label>
          <label>
            Selling price
            <input required min="0" step="0.01" type="number" value={form.sellingPrice} onChange={(event) => setForm({ ...form, sellingPrice: event.target.value })} />
          </label>
          <div className="form-actions">
            <button className="primary-button" disabled={saving} type="submit">
              {saving ? 'Saving...' : editingId === null ? 'Add product' : 'Save changes'}
            </button>
            {editingId !== null && <button className="secondary-button" type="button" onClick={cancelEdit}>Cancel</button>}
          </div>
        </form>
      </section>

      <section className="panel product-list-panel">
        <div className="panel-heading">
          <div>
            <p className="eyebrow">Active catalog</p>
            <h2>Products</h2>
          </div>
          <span className="item-count">{products.length} product{products.length === 1 ? '' : 's'}</span>
        </div>
        {loading ? (
          <p className="empty-state">Loading products...</p>
        ) : products.length === 0 ? (
          <p className="empty-state">No active products found.</p>
        ) : (
          <div className="table-wrapper">
            <table>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Code</th>
                  <th>Category</th>
                  <th>Cost price</th>
                  <th>Selling price</th>
                  <th><span className="visually-hidden">Actions</span></th>
                </tr>
              </thead>
              <tbody>
                {products.map((product) => (
                  <tr key={product.id}>
                    <td>{product.name}</td>
                    <td>{product.code}</td>
                    <td>{product.category || '-'}</td>
                    <td>{product.costPrice.toFixed(2)}</td>
                    <td>{product.sellingPrice.toFixed(2)}</td>
                    <td className="row-actions">
                      <button className="text-button" type="button" onClick={() => handleEdit(product)}>Edit</button>
                      <button className="text-button danger" type="button" onClick={() => void handleDelete(product)}>Delete</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </section>
  )
}

export default Products
