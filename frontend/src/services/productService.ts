import { productApi } from "./api";

export async function getProducts() {
  const response = await productApi.get(
    "/api/products"
  );

  return response.data;
}

export async function getProduct(id: string) {
  const response = await productApi.get(
    `/api/products/${id}`
  );

  return response.data;
}
