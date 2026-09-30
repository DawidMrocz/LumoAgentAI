import { createBrowserRouter } from 'react-router-dom';

import Home from '../pages/Home/Home';
import Login from '../pages/Login/Login';
import Register from '../pages/Register/Register';
import ForgotPassword from '../pages/ForgotPassword/ForgotPassword';
import ResetPassword from '../pages/ResetPassword/ResetPassword';
import ConfirmEmail from '../pages/ConfirmEmail/ConfirmEmail';

import ProtectedRoute from '../ProtectedRoute';
import MainLayout from '../layout/MainLayout';
import AuthLayout from '../layout/AuthLayout';

// OTHER
import Profile from '../pages/Profile/Profile';
import ChangePassword from '../pages/ChangePassword/ChangePassword';

import Chat from '../pages/Chat/Chat';
import Documents from '../pages/Documents/Documents';
import Payments from '../pages/Payments/Payments';

// META
import MetaCallback from '../pages/MetaCallback/MetaCallback';

export const router = createBrowserRouter([
	{
		element: <AuthLayout />,
		children: [
			// ======================
			// PUBLIC ROUTES
			// ======================
			{ path: '/login', element: <Login /> },
			{ path: '/register', element: <Register /> },
			{ path: '/forgot-password', element: <ForgotPassword /> },
			{ path: '/reset-password', element: <ResetPassword /> },
			{ path: '/confirm-email', element: <ConfirmEmail /> },

			// ======================
			// META CALLBACK
			// ======================
			{
				path: '/meta/callback',
				element: <MetaCallback />,
			},

			// ======================
			// PROTECTED APP
			// ======================
			{
				element: (
					<ProtectedRoute>
						<MainLayout />
					</ProtectedRoute>
				),
				children: [
					{ path: '/', element: <Home /> },

					{ path: '/chat', element: <Chat /> },
					{ path: '/documents', element: <Documents /> },
					{ path: '/payments', element: <Payments /> },

					// PROFILE
					{ path: '/profile', element: <Profile /> },

					{ path: '/change-password', element: <ChangePassword /> },
				],
			},

			{ path: '/change-password', element: <ChangePassword /> },
		],
	},
]);
