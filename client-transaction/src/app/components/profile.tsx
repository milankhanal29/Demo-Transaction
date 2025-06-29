'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { fetchWithToken } from '../utils/fetchWithToken';

interface UserData {
  id: string;
  name: string;
  fullName: string;

  role: string;
  balance: number;
}

export default function Profile() {
  const { token, user } = useAuth();
  const [userData, setUserData] = useState<UserData | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;

    try {
      const decoded = JSON.parse(atob(token.split('.')[1]));
      const userId = decoded.sub || decoded.userId || decoded.id;

      fetchWithToken(`https://localhost:7200/api/users/${userId}`, token)
        .then(async (res) => {
          if (!res.ok) {
            throw new Error('Failed to fetch user data');
          }
          const data: UserData = await res.json();
          setUserData(data);
        })
        .catch((err) => {
          console.error(err);
          setError('Could not load user profile');
        });
    } catch (err) {
      console.error('Token decode error:', err);
      setError('Invalid token');
    }
  }, [token]);

  if (error) return <div className="text-red-500">{error}</div>;
  if (!userData) return <div>Loading profile...</div>;

  return (
    <div className=" shadow-md rounded p-4 max-w-md">
      <h2 className="text-xl font-semibold mb-2">User Profile</h2>
      <p><strong>Full Name:</strong> {userData.name}</p>
      <p><strong>Role:</strong> {userData.role}</p>
      <p><strong>Balance:</strong> {userData.balance}</p>
    </div>
  );
}
