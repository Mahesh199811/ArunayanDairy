import axios from "axios";

export const userApi = axios.create({
  baseURL: "http://localhost:5080",
});

export const productApi = axios.create({
  baseURL: "http://localhost:5001",
});

export const orderApi = axios.create({
  baseURL: "http://localhost:5002",
});
