'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/app/context/AuthContext';
import toast from 'react-hot-toast';

export default function RegisterPage() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [accountNumber, setAccountNumber] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);

  const router = useRouter();
  const { login } = useAuth();

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    try {
      const res = await fetch('https://localhost:7200/api/users/register', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name,
          email,
          password,
          accountNumber,
        }),
      });

      const data = await res.json();
      if (!res.ok) {
      if (data.errors) {
        const messages = Object.entries(data.errors)
          .map(([field, messages]) => `${field}: ${(messages as string[]).join(', ')}`)
          .join('\n');
        setError(messages);
      } else {
        setError(data.message || 'Registration failed');
      }
      return;
    }

      if (!res.ok || !data.accessToken) {
        setError(data.message || 'Registration failed');
        return;
      }

      login(data.accessToken);
      toast.success("Successfully registered");
      router.push('/');
      router.refresh();
    } catch (err) {
      setError('Something went wrong during registration');
      toast.success("failed to register");

      console.error(err);
    }
  };

  return (
    <div className="max-w-md mx-auto mt-10 p-6 shadow-md rounded">
      <h2 className="text-2xl font-bold mb-6 text-center">Register</h2>

      {error && <p className="text-red-500 text-center mb-4">{error}</p>}

      <form onSubmit={handleRegister} className="space-y-4">
        <input
          className="w-full border p-2 rounded"
          placeholder="Full Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
        <input
          className="w-full border p-2 rounded"
          placeholder="Email"
          value={email}
          type="email"
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <input
          className="w-full border p-2 rounded"
          placeholder="Account Number"
          value={accountNumber}
          onChange={(e) => setAccountNumber(e.target.value)}
          required
        />
        <input
          className="w-full border p-2 rounded"
          placeholder="Password"
          value={password}
          type="password"
          onChange={(e) => setPassword(e.target.value)}
          required
        />
   

        <button
          type="submit"
          className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700"
        >
          Register
        </button>
      </form>
    </div>
  );
}
