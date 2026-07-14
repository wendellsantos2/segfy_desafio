import { createBrowserRouter, Navigate } from 'react-router-dom';
import { Layout } from './components/layout/Layout';
import { SinistrosList } from './components/sinistros/SinistrosList';
import { SinistroDetail } from './components/sinistros/SinistroDetail';
import { ApolicesList } from './components/apolices/ApolicesList';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      {
        index: true,
        element: <Navigate to="/sinistros" replace />,
      },
      {
        path: 'sinistros',
        element: <SinistrosList />,
      },
      {
        path: 'sinistros/:id',
        element: <SinistroDetail />,
      },
      {
        path: 'apolices',
        element: <ApolicesList />,
      }
    ],
  },
]);
