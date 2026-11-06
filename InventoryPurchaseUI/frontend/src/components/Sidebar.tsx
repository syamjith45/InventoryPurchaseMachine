import { NavLink } from 'react-router-dom'

const menuItems = [
  { label: 'Dashboard', path: '/', icon: '▦' },
  { label: 'Products', path: '/products', icon: '▤' },
  { label: 'Suppliers', path: '/suppliers', icon: '♙' },
  { label: 'Taxes', path: '/taxes', icon: '%' },
  { label: 'New Purchase', path: '/purchases/new', icon: '+' },
  { label: 'Purchases', path: '/purchases', icon: '↔' },
  { label: 'Inventory', path: '/inventory', icon: '▥' },
]

function Sidebar() {
  return (
    <aside className="sidebar">
      <NavLink className="brand" to="/">
        <span className="brand-mark">IP</span>
        <span>InventoryPro</span>
      </NavLink>
      <p className="menu-label">Main menu</p>
      <nav aria-label="Sidebar navigation">
        {menuItems.map((item) => (
          <NavLink
            key={item.path}
            className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}
            to={item.path}
            end={item.path === '/'}
          >
            <span className="menu-icon" aria-hidden="true">{item.icon}</span>
            {item.label}
          </NavLink>
        ))}
      </nav>
      <div className="sidebar-footer">
        <p>Inventory Purchase Management</p>
        <span>Version 1.0</span>
      </div>
    </aside>
  )
}

export default Sidebar
