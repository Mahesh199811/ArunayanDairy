import { useEffect, useState } from "react";
import { getUserOrders } from "../services/orderService";
import { readStoredUser } from "../lib/userStorage";

interface OrderItem {
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

interface Order {
  id: string;
  scheduledDate: string;
  totalAmount: number;
  status: string;
  createdAt: string;
  items: OrderItem[];
}

export default function Orders() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [missingUser, setMissingUser] = useState(false);

  useEffect(() => {
    loadOrders();
  }, []);

  async function loadOrders() {
    const user = readStoredUser();

    if (!user) {
      setMissingUser(true);
      return;
    }

    try {
      const data = await getUserOrders(user.id);
      setOrders(Array.isArray(data) ? data : []);
    } catch (error) {
      console.error(error);
      setOrders([]);
    }
  }

  return (
    <section className="workspace">
      <p className="eyebrow">History</p>
      <h2>My Orders</h2>

      {missingUser && (
        <p className="lede">Sign in to see orders placed with your account.</p>
      )}

      {!missingUser && orders.length === 0 && (
        <p className="lede">No orders found yet.</p>
      )}

      <div className="order-list">
        {orders.map((order) => (
          <article key={order.id} className="order-card">
            <div className="order-head">
              <h3>Order {(order.id ?? "").slice(0, 8) || "—"}</h3>
              <span className="status">{order.status}</span>
            </div>

            <p>
              Scheduled:{" "}
              {new Date(order.scheduledDate).toLocaleDateString("en-IN", {
                day: "numeric",
                month: "short",
                year: "numeric",
              })}
            </p>

            { (order.items ?? []).map((item, index) => (
              <p key={index}>
                {item.productName} × {item.quantity} = ₹{item.totalPrice}
              </p>
            ))}

            <strong>Total: ₹{order.totalAmount}</strong>
          </article>
        ))}
      </div>
    </section>
  );
}
