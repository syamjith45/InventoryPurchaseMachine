import { useCallback, useEffect, useState } from 'react'
import api from '../services/api'
import type { Purchase } from '../types/api'

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const data = (error as { response?: { data?: unknown } }).response?.data
    if (typeof data === 'string') return data
    if (typeof data === 'object' && data !== null && 'message' in data) return String((data as { message: unknown }).message)
  }
  return 'The request could not be completed.'
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString()
}

function Purchases() {
  const [purchases, setPurchases] = useState<Purchase[]>([])
  const [selectedPurchase, setSelectedPurchase] = useState<Purchase | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [postingId, setPostingId] = useState<number | null>(null)

  const loadPurchases = useCallback(async () => {
    try {
      setLoading(true)
      setError('')
      const response = await api.get<Purchase[]>('/purchases')
      setPurchases(response.data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void loadPurchases()
  }, [loadPurchases])

  const viewDetails = async (id: number) => {
    try {
      setError('')
      const response = await api.get<Purchase>(`/purchases/${id}`)
      setSelectedPurchase(response.data)
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }

  const postPurchase = async (purchase: Purchase) => {
    try {
      setPostingId(purchase.id)
      setError('')
      await api.post(`/purchases/${purchase.id}/post`)
      setSelectedPurchase(null)
      await loadPurchases()
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setPostingId(null)
    }
  }

  return (
    <section>
      <div className="page-heading"><div><p className="eyebrow">Purchasing</p><h1>Purchases</h1><p className="page-description">Review Draft and Posted purchases.</p></div></div>
      {error && <p className="error-message" role="alert">{error}</p>}
      <section className="panel">
        <div className="panel-heading"><div><p className="eyebrow">Purchase list</p><h2>Purchases</h2></div></div>
        {loading ? <p className="empty-state">Loading purchases...</p> : purchases.length === 0 ? <p className="empty-state">No purchases found.</p> : (
          <div className="table-wrapper"><table><thead><tr><th>Invoice</th><th>Supplier</th><th>Invoice date</th><th>Grand total</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody>{purchases.map((purchase) => <tr key={purchase.id}><td>{purchase.invoiceNumber}</td><td>{purchase.supplierName}</td><td>{formatDate(purchase.invoiceDate)}</td><td>{purchase.grandTotal.toFixed(2)}</td><td><span className={`status-badge ${purchase.status.toLowerCase() === 'posted' ? 'posted' : 'draft'}`}>{purchase.status}</span></td><td className="row-actions"><button className="text-button" type="button" onClick={() => void viewDetails(purchase.id)}>View details</button>{purchase.status === 'Draft' && <button className="text-button" disabled={postingId === purchase.id} type="button" onClick={() => void postPurchase(purchase)}>{postingId === purchase.id ? 'Posting...' : 'Post purchase'}</button>}</td></tr>)}</tbody>
          </table></div>
        )}
      </section>
      {selectedPurchase && <section className="panel purchase-details-panel"><div className="panel-heading"><div><p className="eyebrow">Purchase details</p><h2>{selectedPurchase.invoiceNumber}</h2></div><button className="secondary-button" type="button" onClick={() => setSelectedPurchase(null)}>Close</button></div><p className="details-line"><strong>Supplier:</strong> {selectedPurchase.supplierName} &nbsp; <strong>Date:</strong> {formatDate(selectedPurchase.invoiceDate)} &nbsp; <strong>Status:</strong> {selectedPurchase.status}</p><div className="table-wrapper"><table><thead><tr><th>Product</th><th>Lot</th><th>Expiry</th><th>Quantity</th><th>Rate</th><th>Discount</th><th>Tax</th><th>Line total</th></tr></thead><tbody>{selectedPurchase.items.map((item) => <tr key={item.id}><td>{item.productName}</td><td>{item.lotNumber}</td><td>{formatDate(item.expiryDate)}</td><td>{item.quantity}</td><td>{item.purchaseRate.toFixed(2)}</td><td>{item.discountPercentage.toFixed(2)}%</td><td>{item.taxAmount.toFixed(2)}</td><td>{item.lineTotal.toFixed(2)}</td></tr>)}</tbody></table></div></section>}
    </section>
  )
}

export default Purchases
