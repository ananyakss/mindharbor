import { User } from './user.model';

export interface AuthResponse {
  accessToken: string;
  user: User;
}

export interface SignInPayload {
  email: string;
  password: string;
}

export interface SignUpPayload extends SignInPayload {
  username: string;
}

