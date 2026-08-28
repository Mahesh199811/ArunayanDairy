import { useEffect, useMemo, useState } from "react";
import axios from "axios";
import { useCart } from "../context/CartContext";
import { createOrder } from "../services/orderService";

interface CartProps {
  onOrderPlaced: () => void;
}

function toDateInput(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return "";
  }

  return date.toISOString().slice(0, 10);
}

export default function Cart({ onOrderPlaced }: CartProps) {
  const {
    items,
    updateQuantity,
    removeFromCart,
    clearCart,
  } = useCart();

  const suggestedDate = useMemo(
    () => (items[0] ? toDateInput(items[0].availableDate) : ""),
    [items]
  );

  const [scheduledDate, setScheduledDate] = useState(suggestedDate);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (suggestedDate) {
      setScheduledDate(suggestedDate);
    }
  }, [suggestedDate]);

  const userJson = localStorage.getItem("user");
  const user = userJson ? JSON.parse(userJson) : null;

  const totalAmount = items.reduce(
    (total, item) => total + item.price * item.quantity,
    0
  );

  async function handleOrder() {
    setError("");

    if (!user) {
      setError("Please sign in before placing an order.");
      return;
    }

    if (items.length === 0) {
      setError("Your cart is empty.");
      return;
    }

    if (!scheduledDate) {
      setError("Please select a scheduled date.");
      return;
    }

    setLoading(true);

    try {
      await createOrder({
        userId: user.id,
        scheduledDate: `${scheduledDate}T00:00:00Z`,
        items: items.map((item) => ({
          productId: item.productId,
          quantity: item.quantity,
        })),
      });

      clearCart();
      onOrderPlaced();
    } catch (err) {
      console.error(err);

      if (axios.isAxiosError(err) && typeof err.response?.data === "string") {
        setError(err.response.data);
      } else {
        setError("Unable to place order. Check the date matches product availability.");
      }
    } finally {
      setLoading(false);
    }
  }

  if (items.length === 0) {
    return (
      <section className="workspace">
        <p className="eyebrow">Cart</p>
        <h2>Your cart is empty</h2>
        <p className="lede">Add milk, curd, or paneer from today's dairy.</p>
      </section>
    );
  }

  return (
    <section className="workspace">
      <p className="eyebrow">Cart</p>
      <h2>Schedule your order</h2>
      <p className="lede">
        Choose quantity and a delivery date that matches the product available date.
      </p>

      <div className="cart-list">
        {items.map((item) => (
          <article key={item.productId} className="cart-row">
            <div>
              <h3>{item.name}</h3>
              <p>
                ₹{item.price} / {item.unit}
              </p>
            </div>

            <label>
              Qty
              <input
                type="number"
                min="1"
                max={item.availableQuantity}
                value={item.quantity}
                onChange={(e) =>
                  updateQuantity(item.productId, Number(e.target.value))
                }
              />
            </label>

            <p className="cart-total">₹{item.price * item.quantity}</p>

            <button
              type="button"
              className="text-button"
              onClick={() => removeFromCart(item.productId)}
            >
              Remove
            </button>
          </article>
        ))}
      </div>

      <div className="checkout">
        <h3>Total: ₹{totalAmount}</h3>

        <label>
          Schedule date
          <input
            type="date"
            value={scheduledDate}
            onChange={(e) => setScheduledDate(e.target.value)}
          />
        </label>

        {error && <p className="form-error">{error}</p>}

        <button
          type="button"
          className="btn-primary"
          onClick={handleOrder}
          disabled={loading}
        >
          {loading ? "Placing order…" : "Place Order"}
        </button>
      </div>
    </section>
  );
}
