import { useCallback, useEffect, useState } from 'react'
import api from '../services/api'
import type { InventoryItem } from '../types/api'
import type { Product } from '../types/product'

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const data = (error as { response?: { data?: unknown } }).response?.data
    if (typeof data === 'string') return data
    if (typeof data === 'object' && data !== null && 'message' in data) return String((data as { message: unknown }).message)
  }
  return 'The request could not be completed.'
}

function Inventory() {
  const [inventory, setInventory] = useState<InventoryItem[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [selectedProductId, setSelectedProductId] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const loadProducts = useCallback(async () => {
    try {
      const response = await api.get<Product[]>('/products')
      setProducts(response.data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }, [])

  const loadInventory = useCallback(async (productId?: string) => {
    try {
      setLoading(true)
      setError('')
      const path = productId ? `/inventories/${productId}` : '/inventories'
      const response = await api.get<InventoryItem[]>(path)
      setInventory(response.data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void Promise.all([loadProducts(), loadInventory()])
  }, [loadInventory, loadProducts])

  const handleProductFilter = (productId: string) => {
    setSelectedProductId(productId)
    void loadInventory(productId || undefined)
  }

  return (
    <section>
      <div className="page-heading"><div><p className="eyebrow">Stock</p><h1>Inventory</h1><p className="page-description">View available, non-expired inventory lots.</p></div></div>
      {error && <p className="error-message" role="alert">{error}</p>}
      <section className="panel inventory-filter"><label>Filter by product<select value={selectedProductId} onChange={(event) => handleProductFilter(event.target.value)}><option value="">All products</option>{products.map((product) => <option key={product.id} value={product.id}>{product.name} ({product.code})</option>)}</select></label></section>
      <section className="panel"><div className="panel-heading"><div><p className="eyebrow">Available lots</p><h2>Inventory</h2></div></div>{loading ? <p className="empty-state">Loading inventory...</p> : inventory.length === 0 ? <p className="empty-state">No inventory records found.</p> : <div className="table-wrapper"><table><thead><tr><th>Product</th><th>Product code</th><th>Lot number</th><th>Quantity</th><th>Expiry date</th></tr></thead><tbody>{inventory.map((item) => { const product = products.find((candidate) => candidate.id === item.productId); return <tr key={item.id}><td>{item.productName}</td><td>{product?.code ?? '-'}</td><td>{item.lotNumber}</td><td>{item.quantity}</td><td>{new Date(item.expiryDate).toLocaleDateString()}</td></tr> })}</tbody></table></div>}</section>
    </section>
  )
}

export default Inventory
