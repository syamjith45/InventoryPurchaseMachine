export interface Supplier {
  id: number
  name: string
  contactPerson: string
  phone: string
  email: string
  address: string
  isActive: boolean
}

export interface Tax {
  id: number
  name: string
  percentage: number
  isCompound: boolean
  isActive: boolean
}

export interface PurchaseItem {
  id: number
  productId: number
  productName: string
  lotNumber: string
  expiryDate: string
  quantity: number
  purchaseRate: number
  discountPercentage: number
  taxAmount: number
  lineTotal: number
}

export interface Purchase {
  id: number
  supplierId: number
  supplierName: string
  invoiceNumber: string
  invoiceDate: string
  status: string
  subTotal: number
  totalDiscount: number
  totalTax: number
  grandTotal: number
  createdAt: string
  items: PurchaseItem[]
}

export interface InventoryItem {
  id: number
  productId: number
  productName: string
  lotNumber: string
  expiryDate: string
  quantity: number
}
