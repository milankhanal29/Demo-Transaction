'use client';

import React, { useState } from 'react';
import { useAuth } from '@/app/context/AuthContext';
import toast from 'react-hot-toast';

type TransferItem = {
  userAccountNumber: string;
  amount: number | '';
};

export default function TransferBulk() {
  const { token } = useAuth();

  const [merchantAccountNumber, setMerchantAccountNumber] = useState('');
  const [transfers, setTransfers] = useState<TransferItem[]>([
    { userAccountNumber: '', amount: '' },
  ]);
  const [excelFile, setExcelFile] = useState<File | null>(null);
  const [mode, setMode] = useState<'manual' | 'excel'>('manual'); 

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const MAX_FILE_SIZE = 2 * 1024 * 1024; 

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setError(null);
    const file = e.target.files ? e.target.files[0] : null;

    if (!file) {
      setExcelFile(null);
      return;
    }

    if (file.size > MAX_FILE_SIZE) {
      setError('File size must be less than 2 MB');
      setExcelFile(null);
      return;
    }

    const ext = file.name.split('.').pop()?.toLowerCase();
    if (ext !== 'xlsx' && ext !== 'xls') {
      setError('Only .xlsx or .xls files are allowed');
      setExcelFile(null);
      return;
    }

    setExcelFile(file);
  };

  const handleAddTransfer = () => {
    setTransfers([...transfers, { userAccountNumber: '', amount: '' }]);
  };

  const handleRemoveTransfer = (index: number) => {
    setTransfers(transfers.filter((_, i) => i !== index));
  };

  const handleTransferChange = (
    index: number,
    field: 'userAccountNumber' | 'amount',
    value: string
  ) => {
    const newTransfers = [...transfers];
    if (field === 'amount') {
      newTransfers[index][field] = value === '' ? '' : Number(value);
    } else {
      newTransfers[index][field] = value;
    }
    setTransfers(newTransfers);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!merchantAccountNumber) {
      setError('Your account number is required.');
      return;
    }

    if (!token) {
      setError('Not authenticated');
      return;
    }

    setLoading(true);

    try {
      if (mode === 'excel') {
        if (!excelFile) {
          setError('Please upload an Excel file');
          setLoading(false);
          return;
        }

        const formData = new FormData();
        formData.append('file', excelFile);

        const res = await fetch('https://localhost:7200/api/transactions/upload-transfer', {
          method: 'POST',
          headers: {
            Authorization: `Bearer ${token}`,
          },
          body: formData,
        });

        if (!res.ok) {
          const data = await res.json().catch(() => ({}));
          throw new Error(data?.message || 'Excel upload failed');
        }

        toast.success('Transfer from Excel successful');
        setExcelFile(null);
      } else {
        // manual mode
        const validTransfers = transfers.filter(
          t =>
            t.userAccountNumber.trim() !== '' &&
            t.amount !== '' &&
            Number(t.amount) > 0
        );

        if (validTransfers.length === 0) {
          setError('Please add at least one transfer');
          setLoading(false);
          return;
        }

        const payload = {
          merchantAccountNumber,
          transfers: validTransfers.map(t => ({
            userAccountNumber: t.userAccountNumber.trim(),
            amount: Number(t.amount),
          })),
        };

        const res = await fetch('https://localhost:7200/api/transactions/transfer', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify(payload),
        });

        if (!res.ok) {
          const data = await res.json().catch(() => ({}));
          throw new Error(data?.message || 'Transfer failed');
        }

        toast.success('Bulk transfer successful');
        setTransfers([{ userAccountNumber: '', amount: '' }]);
      }
    } catch (err: any) {
      setError(err.message || 'Unknown error occurred');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto p-4 border rounded shadow">
      <h2 className="text-xl font-bold mb-4">Bulk Transfer</h2>

      <div className="mb-4 space-x-4">
        <label className="inline-flex items-center">
          <input
            type="radio"
            name="mode"
            value="manual"
            checked={mode === 'manual'}
            onChange={() => {
              setMode('manual');
              setExcelFile(null);
              setError(null);
            }}
            className="mr-2"
          />
          Manual Input
        </label>

        <label className="inline-flex items-center">
          <input
            type="radio"
            name="mode"
            value="excel"
            checked={mode === 'excel'}
            onChange={() => {
              setMode('excel');
              setTransfers([{ userAccountNumber: '', amount: '' }]);
              setError(null);
            }}
            className="mr-2"
          />
          Upload Excel File
        </label>
      </div>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block font-semibold mb-1">Your Account Number</label>
          <input
            type="text"
            value={merchantAccountNumber}
            onChange={e => setMerchantAccountNumber(e.target.value)}
            className="w-full border px-3 py-2 rounded"
            placeholder="Enter your account number"
            disabled={loading}
          />
        </div>

        {mode === 'excel' ? (
          <div>
            <label className="block font-semibold mb-1">Excel File (.xlsx, .xls) &lt; 2MB</label>
            <input
              type="file"
              accept=".xlsx,.xls"
              onChange={handleFileChange}
              disabled={loading}
              className="w-full"
            />
          </div>
        ) : (
          <>
            <label className="block font-semibold mb-1">Transfers</label>
            {transfers.map((transfer, i) => (
              <div key={i} className="flex gap-2 mb-2">
                <input
                  type="text"
                  placeholder="Receiver Account Number"
                  value={transfer.userAccountNumber}
                  onChange={e => handleTransferChange(i, 'userAccountNumber', e.target.value)}
                  className="flex-1 border px-3 py-2 rounded"
                  disabled={loading}
                />
                <input
                  type="number"
                  min="0"
                  placeholder="Amount"
                  value={transfer.amount}
                  onChange={e => handleTransferChange(i, 'amount', e.target.value)}
                  className="w-24 border px-3 py-2 rounded"
                  disabled={loading}
                />
                <button
                  type="button"
                  onClick={() => handleRemoveTransfer(i)}
                  disabled={loading || transfers.length === 1}
                  className="text-red-600 font-bold px-2"
                >
                  &times;
                </button>
              </div>
            ))}

            <button
              type="button"
              onClick={handleAddTransfer}
              disabled={loading}
              className="text-blue-600 font-semibold"
            >
              + Add another transfer
            </button>
          </>
        )}

        {error && <p className="text-red-600">{error}</p>}

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white font-semibold py-2 rounded"
        >
          {loading ? 'Processing...' : 'Send Bulk Transfer'}
        </button>
      </form>
    </div>
  );
}
