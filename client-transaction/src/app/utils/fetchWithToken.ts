export const fetchWithToken = async (
  url: string,
  token: string,
  options: RequestInit = {}
) => {
  return fetch(url, {
    ...options,
    headers: {
      ...(options.headers || {}),
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
  });
};