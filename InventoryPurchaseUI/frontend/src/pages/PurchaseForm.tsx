import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import api from '../services/api'
import type { Product } from '../types/product'
import type { Purchase, Supplier, Tax } from '../types/api'

interface PurchaseItemForm {
  productId: string
  expiryDate: string
  quantity: string
  purchaseRate: string
  discountPercentage: string
}

const createItem = (): PurchaseItemForm => ({
  productId: '',
  expiryDate: '',
  quantity: '',
  purchaseRate: '',
  discountPercentage: '0',
})

const today = new Date().toISOString().slice(0, 10)

function getErrorMessage(error: unknown) {
  if (typeof error === 'object' && error !== null && 'response' in error) {
    const data = (error as { response?: { data?: unknown } }).response?.data
    if (typeof data === 'string') return data
    if (typeof data === 'object' && data !== null && 'message' in data) return String((data as { message: unknown }).message)
  }
  return 'The purchase could not be saved.'
}

function calculateTax(amount: number, taxes: Tax[]) {
  let totalTax = 0
  const simpleTaxes = taxes.filter((tax) => !tax.isCompound)
  simpleTaxes.forEach((tax) => {
    totalTax += amount * tax.percentage / 100
  })

  let compoundAmount = amount + totalTax
  taxes.filter((tax) => tax.isCompound).forEach((tax) => {
    const taxAmount = compoundAmount * tax.percentage / 100
    totalTax += taxAmount
    compoundAmount += taxAmount
  })
  return totalTax
}

function PurchaseForm() {
  const [suppliers, setSuppliers] = useState<Supplier[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [productTaxes, setProductTaxes] = useState<Record<number, Tax[]>>({})
  const [supplierId, setSupplierId] = useState('')
  const [invoiceDate, setInvoiceDate] = useState(today)
  const [items, setItems] = useState<PurchaseItemForm[]>([createItem()])
  const [createdPurchase, setCreatedPurchase] = useState<Purchase | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    const loadOptions = async () => {
      try {
        setLoading(true)
        const [supplierResponse, productResponse] = await Promise.all([
          api.get<Supplier[]>('/suppliers'),
          api.get<Product[]>('/products'),
        ])
        setSuppliers(supplierResponse.data)
        setProducts(productResponse.data)
      } catch (requestError) {
        setError(getErrorMessage(requestError))
      } finally {
        setLoading(false)
      }
    }
    void loadOptions()
  }, [])

  const totals = useMemo(() => items.reduce((result, item) => {
    const quantity = Number(item.quantity) || 0
    const rate = Number(item.purchaseRate) || 0
    const discountPercentage = Number(item.discountPercentage) || 0
    const subtotal = quantity * rate
    const discount = subtotal * discountPercentage / 100
    const taxableAmount = subtotal - discount
    const tax = item.productId ? calculateTax(taxableAmount, productTaxes[Number(item.productId)] ?? []) : 0
    result.subtotal += subtotal
    result.discount += discount
    result.tax += tax
    return result
  }, { subtotal: 0, discount: 0, tax: 0 }), [items, productTaxes])

  const updateItem = (index: number, changes: Partial<PurchaseItemForm>) => {
    setItems((current) => current.map((item, itemIndex) => itemIndex === index ? { ...item, ...changes } : item))
  }

  const loadProductTaxes = async (productId: number) => {
    if (productTaxes[productId]) return
    try {
      const response = await api.get<Tax[]>(`/products/${productId}/taxes`)
      setProductTaxes((current) => ({ ...current, [productId]: response.data }))
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    }
  }

  const handleProductChange = (index: number, productId: string) => {
    updateItem(index, { productId })
    if (productId) void loadProductTaxes(Number(productId))
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setSuccess('')

    if (!supplierId || !invoiceDate) {
      setError('Supplier and invoice date are required.')
      return
    }
    if (items.some((item) => !item.productId || !item.expiryDate || Number(item.quantity) <= 0 || Number(item.purchaseRate) < 0 || Number(item.discountPercentage) < 0 || Number(item.discountPercentage) > 100)) {
      setError('Complete each item with a product, expiry date, positive quantity, valid rate, and discount from 0 to 100.')
      return
    }

    try {
      setSaving(true)
      const response = await api.post<Purchase>('/purchases', {
        supplierId: Number(supplierId),
        invoiceDate,
        items: items.map((item) => {
          const quantity = Number(item.quantity)
          const rate = Number(item.purchaseRate)
          const discount = Number(item.discountPercentage)
          return {
            productId: Number(item.productId),
            expiryDate: item.expiryDate,
            quantity,
            purchaseRate: rate,
            discountPercentage: discount,
          }
        }),
      })
      setCreatedPurchase(response.data)
      setSuccess(`Purchase ${response.data.invoiceNumber} created as Draft.`)
      setSupplierId('')
      setInvoiceDate(today)
      setItems([createItem()])
    } catch (requestError) {
      setError(getErrorMessage(requestError))
    } finally {
      setSaving(false)
    }
  }

  return (
    <section>
      <div className="page-heading"><div><p className="eyebrow">Purchasing</p><h1>New Purchase</h1><p className="page-description">Create a purchase in Draft status before posting it to inventory.</p></div></div>
      {error && <p className="error-message" role="alert">{error}</p>}
      {success && <p className="success-message" role="status">{success}</p>}
      <form onSubmit={handleSubmit}>
        <section className="panel form-panel">
          <div className="entity-form purchase-header-form">
            <label>Supplier<select required disabled={loading} value={supplierId} onChange={(event) => setSupplierId(event.target.value)}><option value="">Select supplier</option>{suppliers.map((supplier) => <option key={supplier.id} value={supplier.id}>{supplier.name}</option>)}</select></label>
            <label>Invoice date<input required type="date" value={invoiceDate} onChange={(event) => setInvoiceDate(event.target.value)} /></label>
          </div>
        </section>
        <section className="panel purchase-items-panel">
          <div className="panel-heading"><div><p className="eyebrow">Line items</p><h2>Purchase items</h2></div><button className="secondary-button" type="button" onClick={() => setItems([...items, createItem()])}>Add item</button></div>
          <div className="purchase-items">
            {items.map((item, index) => <div className="purchase-item" key={index}>
              <div className="item-heading"><strong>Item {index + 1}</strong>{items.length > 1 && <button className="text-button danger" type="button" onClick={() => setItems(items.filter((_, itemIndex) => itemIndex !== index))}>Remove</button>}</div>
              <div className="item-form">
                <label>Product<select required value={item.productId} onChange={(event) => handleProductChange(index, event.target.value)}><option value="">Select product</option>{products.map((product) => <option key={product.id} value={product.id}>{product.name} ({product.code})</option>)}</select></label>
                <label>Expiry date<input required type="date" value={item.expiryDate} onChange={(event) => updateItem(index, { expiryDate: event.target.value })} /></label>
                <label>Quantity<input required min="0.01" step="0.01" type="number" value={item.quantity} onChange={(event) => updateItem(index, { quantity: event.target.value })} /></label>
                <label>Purchase rate<input required min="0" step="0.01" type="number" value={item.purchaseRate} onChange={(event) => updateItem(index, { purchaseRate: event.target.value })} /></label>
                <label>Discount %<input required min="0" max="100" step="0.01" type="number" value={item.discountPercentage} onChange={(event) => updateItem(index, { discountPercentage: event.target.value })} /></label>
              </div>
            </div>)}
          </div>
        </section>
        <section className="panel purchase-total-panel">
          <div className="totals"><span>Subtotal <strong>{totals.subtotal.toFixed(2)}</strong></span><span>Discount <strong>{totals.discount.toFixed(2)}</strong></span><span>Tax <strong>{totals.tax.toFixed(2)}</strong></span><span className="grand-total">Grand total <strong>{(totals.subtotal - totals.discount + totals.tax).toFixed(2)}</strong></span></div>
          <button className="primary-button" disabled={saving || loading} type="submit">{saving ? 'Creating...' : 'Create Draft purchase'}</button>
        </section>
      </form>
      {createdPurchase && <section className="panel generated-numbers-panel"><div className="panel-heading"><div><p className="eyebrow">Generated numbers</p><h2>Purchase identifiers</h2></div></div><p className="generated-invoice"><strong>Invoice number:</strong> {createdPurchase.invoiceNumber}</p><div className="table-wrapper"><table><thead><tr><th>Product</th><th>Generated lot number</th></tr></thead><tbody>{createdPurchase.items.map((item) => <tr key={item.id}><td>{item.productName}</td><td>{item.lotNumber}</td></tr>)}</tbody></table></div></section>}
    </section>
  )
}

export default PurchaseForm
