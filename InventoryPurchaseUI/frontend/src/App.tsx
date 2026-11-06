import { BrowserRouter, Link, Navigate, Route, Routes } from 'react-router-dom'
import Navbar from './components/Navbar'
import Sidebar from './components/Sidebar'
import Inventory from './pages/Inventory'
import Products from './pages/Products'
import PurchaseForm from './pages/PurchaseForm'
import Purchases from './pages/Purchases'
import Suppliers from './pages/Suppliers'
import Taxes from './pages/Taxes'
import './App.css'

function Dashboard() {
  return (
    <section className="dashboard-page">
      <div className="page-heading">
        <div>
          <p className="eyebrow">Overview</p>
          <h1>Dashboard</h1>
          <p className="page-description">Use the navigation to manage products, suppliers, purchases, taxes, and inventory.</p>
        </div>
        <Link className="primary-button" to="/purchases/new">
          New purchase
        </Link>
      </div>

      <div className="dashboard-grid">
        <section className="panel">
          <div className="panel-heading"><div><p className="eyebrow">Catalog</p><h2>Manage products</h2></div><Link to="/products">Open products</Link></div>
          <p className="page-description">Create and maintain the active product catalog.</p>
        </section>
        <section className="panel">
          <div className="panel-heading"><div><p className="eyebrow">Purchasing</p><h2>Review purchases</h2></div><Link to="/purchases">Open purchases</Link></div>
          <p className="page-description">Review Draft purchases and post them to inventory.</p>
        </section>
        <section className="panel">
          <div className="panel-heading"><div><p className="eyebrow">Stock</p><h2>View inventory</h2></div><Link to="/inventory">Open inventory</Link></div>
          <p className="page-description">View available inventory lots and filter by product.</p>
        </section>
        <section className="panel">
          <div className="panel-heading"><div><p className="eyebrow">Setup</p><h2>Manage suppliers and taxes</h2></div><Link to="/suppliers">Open setup</Link></div>
          <p className="page-description">Maintain suppliers and tax definitions used by purchases.</p>
        </section>
      </div>
    </section>
  )
}

function App() {
  return (
    <BrowserRouter>
      <div className="app-shell">
        <Sidebar />
        <div className="main-area">
          <Navbar />
          <main className="content">
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/products" element={<Products />} />
              <Route path="/suppliers" element={<Suppliers />} />
              <Route path="/taxes" element={<Taxes />} />
              <Route path="/purchases/new" element={<PurchaseForm />} />
              <Route path="/purchases" element={<Purchases />} />
              <Route path="/inventory" element={<Inventory />} />
              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </main>
        </div>
      </div>
    </BrowserRouter>
  )
}

export default App
