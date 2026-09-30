import type { ProfileDTO } from '../../../api/User.api';

export type UserDTO = ProfileDTO;

export interface AuthContextType {
	isLoggedIn: boolean;
	loading: boolean;

	user: UserDTO | null;

	refetch: () => Promise<UserDTO | null>;
	logout: () => Promise<void>;
}
