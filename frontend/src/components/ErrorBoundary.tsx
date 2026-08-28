import { Component, type ErrorInfo, type ReactNode } from "react";

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { hasError: false };

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("App crashed", error, info);
  }

  render() {
    if (this.state.hasError) {
      return (
        <section className="workspace">
          <p className="eyebrow">Arunayan Dairy</p>
          <h2>The page needs a moment</h2>
          <p className="lede">
            Something interrupted the app. Refresh and you can continue from
            products or your cart.
          </p>
          <button
            type="button"
            className="btn-primary"
            onClick={() => window.location.reload()}
          >
            Refresh
          </button>
        </section>
      );
    }

    return this.props.children;
  }
}
