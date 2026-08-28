import { userApi } from "./api";

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export async function registerUser(
  request: RegisterRequest
) {
  const response = await userApi.post(
    "/api/auth/register",
    request
  );

  return response.data;
}

export async function loginUser(
  request: LoginRequest
) {
  const response = await userApi.post(
    "/api/auth/login",
    request
  );

  return response.data;
}
