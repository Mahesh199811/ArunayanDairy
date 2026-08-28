import {
  createContext,
  useContext,
  useState,
  ReactNode,
} from "react";

export interface CartItem {
  productId: string;
  name: string;
  price: number;
  unit: string;
  quantity: number;
  availableQuantity: number;
  availableDate: string;
}

interface CartContextType {
  items: CartItem[];
  addToCart: (item: CartItem) => void;
  removeFromCart: (productId: string) => void;
  updateQuantity: (
    productId: string,
    quantity: number
  ) => void;
  clearCart: () => void;
}

const CartContext =
  createContext<CartContextType | undefined>(
    undefined
  );

export function CartProvider({
  children,
}: {
  children: ReactNode;
}) {
  const [items, setItems] = useState<CartItem[]>(
    []
  );

  function addToCart(item: CartItem) {
    setItems((currentItems) => {
      const existing = currentItems.find(
        (x) => x.productId === item.productId
      );

      if (existing) {
        return currentItems.map((x) =>
          x.productId === item.productId
            ? {
                ...x,
                quantity: Math.min(
                  x.quantity + item.quantity,
                  x.availableQuantity
                ),
              }
            : x
        );
      }

      return [...currentItems, item];
    });
  }

  function removeFromCart(productId: string) {
    setItems((currentItems) =>
      currentItems.filter(
        (x) => x.productId !== productId
      )
    );
  }

  function updateQuantity(
    productId: string,
    quantity: number
  ) {
    setItems((currentItems) =>
      currentItems.map((x) =>
        x.productId === productId
          ? {
              ...x,
              quantity: Math.min(
                Math.max(quantity, 1),
                x.availableQuantity
              ),
            }
          : x
      )
    );
  }

  function clearCart() {
    setItems([]);
  }

  return (
    <CartContext.Provider
      value={{
        items,
        addToCart,
        removeFromCart,
        updateQuantity,
        clearCart,
      }}
    >
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  const context = useContext(CartContext);

  if (!context) {
    throw new Error(
      "useCart must be used inside CartProvider"
    );
  }

  return context;
}
