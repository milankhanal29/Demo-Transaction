'use client';

import React, { useState } from 'react';
import { useAuth } from '@/app/context/AuthContext';
import toast from 'react-hot-toast';

type TransferRequest = {
  merchantAccountNumber: string;
  transfers: { userAccountNumber: string; amount: number }[];
};

export default function Transfer() {
  const { token } = useAuth();
  const [merchantAccountNumber, setMerchantAccountNumber] = useState('');
  const [userAccountNumber, setUserAccountNumber] = useState('');
  const [amount, setAmount] = useState<number | ''>('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const bankName = 'foneinsure bank'; 

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!merchantAccountNumber || !userAccountNumber || !amount) {
      setError('Please fill all fields');
      return;
    }

    if (!token) {
      setError('Not authenticated');
      return;
    }

    const payload: TransferRequest = {
      merchantAccountNumber,
      transfers: [{ userAccountNumber, amount: Number(amount) }],
    };

    setLoading(true);
   try {
  const res = await fetch('https://localhost:7200/api/transactions/transfer', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    let errorMessage = 'Transfer failed';

    try {
      const contentType = res.headers.get('content-type');
      if (contentType?.includes('application/json')) {
        const data = await res.json();
        errorMessage = data.message || errorMessage;
      } else {
        const text = await res.text();
        errorMessage = text || errorMessage;
      }
    } catch {
      errorMessage = 'An unexpected error occurred';
    }

    throw new Error(errorMessage);
  }

  toast.success('Transfer successful');
  setUserAccountNumber('');
  setAmount('');
} catch (err: any) {
  setError(err.message || 'Unknown error');
  toast.error(err.message);
} finally {
  setLoading(false);
}
  };

  return (
    <div className="max-w-md mx-auto p-4 border rounded shadow">
      <h2 className="text-xl font-bold mb-4">Single Transfer</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block font-semibold mb-1">Your Account Number</label>
          <input
            type="text"
            value={merchantAccountNumber}
            onChange={e => setMerchantAccountNumber(e.target.value)}
            className="w-full border px-3 py-2 rounded"
            placeholder="Enter your account number"
          />
        </div>
        <div>
          <label className="block font-semibold mb-1">Receiver Account Number</label>
          <input
            type="text"
            value={userAccountNumber}
            onChange={e => setUserAccountNumber(e.target.value)}
            className="w-full border px-3 py-2 rounded"
            placeholder="Receiver's account number"
          />
        </div>
        <div>
          <label className="block font-semibold mb-1">Bank Name</label>
          <input
            type="text"
            value={bankName}
            readOnly
            className="w-full border px-3 py-2 rounded cursor-not-allowed"
          />
        </div>
        <div>
          <label className="block font-semibold mb-1">Amount</label>
          <input
            type="number"
            min="0"
            value={amount}
            onChange={e => setAmount(e.target.value === '' ? '' : Number(e.target.value))}
            className="w-full border px-3 py-2 rounded"
            placeholder="Enter amount"
          />
        </div>
        {error && <p className="text-red-600">{error}</p>}
        <button
          type="submit"
          disabled={loading}
          className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white font-semibold py-2 rounded"
        >
          {loading ? 'Transferring...' : 'Transfer'}
        </button>
      </form>
    </div>
  );
}
