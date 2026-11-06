export interface Product {
  id: number
  name: string
}

export interface Supplier {
  id: number
  name: string
}

export interface Tax {
  id: number
  name: string
  rate: number
}

export interface Purchase {
  id: number
  supplierId: number
  total: number
}

export interface InventoryItem {
  id: number
  productId: number
  quantity: number
}
