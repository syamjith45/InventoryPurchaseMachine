export interface Product {
  id: number
  name: string
  code: string
  category: string
  costPrice: number
  sellingPrice: number
  isActive: boolean
}

export interface ProductFormData {
  name: string
  code: string
  category: string
  costPrice: string
  sellingPrice: string
}
