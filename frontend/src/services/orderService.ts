import { orderApi } from "./api";

export interface CreateOrderItem {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  userId: string;
  scheduledDate: string;
  items: CreateOrderItem[];
}

export async function createOrder(
  request: CreateOrderRequest
) {
  const response = await orderApi.post(
    "/api/orders",
    request
  );

  return response.data;
}

export async function getUserOrders(
  userId: string
) {
  const response = await orderApi.get(
    `/api/orders/user/${userId}`
  );

  return response.data;
}
