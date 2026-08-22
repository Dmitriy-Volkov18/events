jest.mock('./store', () => ({
    store: {
        commonStore: {
            setToken: jest.fn(),
        },
        modalStore: {
            closeModal: jest.fn(),
        },
    },
}));

jest.mock('../api/agent', () => ({
    __esModule: true,
    default: {
        Account: {
            register: jest.fn(),
            login: jest.fn(),
        },
    },
}));

jest.mock('../router', () => ({
    navigate: jest.fn(),
}));

import UserStore from './userStore';
import agent from '../api/agent';
import { navigate } from '../router';
import { User } from '../models/user';
import { store } from './store';

describe('UserStore.register', () => {
    let userStore: UserStore;

    beforeEach(() => {
        jest.clearAllMocks();

        userStore = new UserStore();
    });

    it('should register user successfully', async () => {
        // Arrange
        const credentials = {
            dispayName: 'Test User',
            username: 'testuser',
            email: 'test@example.com',
            password: 'Password123!',
        };

        const user: User = {
            username: 'testuser',
            dispayName: 'Test User',
            token: 'test-token',
            image: undefined,
        };

        (agent.Account.register as jest.Mock)
            .mockResolvedValue(user);

        // Act
        await userStore.register(credentials);

        // Assert
        expect(agent.Account.register)
            .toHaveBeenCalledTimes(1);

        expect(agent.Account.register)
            .toHaveBeenCalledWith(credentials);

        expect(store.commonStore.setToken)
            .toHaveBeenCalledWith('test-token');

        expect(userStore.user)
            .toEqual(user);

        expect(navigate)
            .toHaveBeenCalledWith('/activities');

        expect(store.modalStore.closeModal)
            .toHaveBeenCalledTimes(1);
    });

    it('should throw error when registration fails', async () => {
        // Arrange
        const credentials = {
            dispayName: 'Test User',
            username: 'testuser',
            email: 'test@example.com',
            password: 'Password123!',
        };

        const error = new Error('Registration failed');

        (agent.Account.register as jest.Mock)
            .mockRejectedValue(error);

        // Act & Assert
        await expect(
            userStore.register(credentials)
        ).rejects.toBe(error);

        expect(userStore.user)
            .toBeNull();

        expect(store.commonStore.setToken)
            .not.toHaveBeenCalled();

        expect(navigate)
            .not.toHaveBeenCalled();

        expect(store.modalStore.closeModal)
            .not.toHaveBeenCalled();
    });
});


describe('UserStore.login', () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it('should login user successfully', async () => {
        // Arrange
        const credentials = {
            email: 'test@example.com',
            password: 'password123'
        };

        const user: User = {
            username: 'testuser',
            dispayName: 'Test User',
            token: 'test-token',
            image: undefined
        };

        (agent.Account.login as jest.Mock)
            .mockResolvedValue(user);

        const userStore = new UserStore();

        // Act
        await userStore.login(credentials);

        // Assert
        expect(agent.Account.login)
            .toHaveBeenCalledTimes(1);

        expect(agent.Account.login)
            .toHaveBeenCalledWith(credentials);

        expect(store.commonStore.setToken)
            .toHaveBeenCalledWith('test-token');

        expect(userStore.user)
            .toEqual(user);

        expect(navigate)
            .toHaveBeenCalledWith('/activities');

        expect(store.modalStore.closeModal)
            .toHaveBeenCalledTimes(1);
    });

    it('should throw error when login fails', async () => {
        // Arrange
        const credentials = {
            email: 'test@example.com',
            password: 'wrong-password'
        };

        const error = new Error('Invalid credentials');

        (agent.Account.login as jest.Mock)
            .mockRejectedValue(error);

        const userStore = new UserStore();

        // Act & Assert
        await expect(
            userStore.login(credentials)
        ).rejects.toBe(error);

        expect(agent.Account.login)
            .toHaveBeenCalledWith(credentials);

        expect(userStore.user)
            .toBeNull();

        expect(store.commonStore.setToken)
            .not.toHaveBeenCalled();

        expect(navigate)
            .not.toHaveBeenCalled();

        expect(store.modalStore.closeModal)
            .not.toHaveBeenCalled();
    });
});