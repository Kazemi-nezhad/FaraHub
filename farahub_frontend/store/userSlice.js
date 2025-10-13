import { createSlice } from "@reduxjs/toolkit";

const initialState = {
  list: [],
  loading: false,
  error: null,
};

const userSlice = createSlice({
  name: "users",
  initialState,
  reducers: {
    fetchUsersStart: (state) => {
      state.loading = true;
      state.error = null;
    },
    fetchUsersSuccess: (state, action) => {
      state.loading = false;
      state.list = action.payload;
    },
    fetchUsersFailure: (state, action) => {
      state.loading = false;
      state.error = action.payload;
    },
    upsertUser: (state, action) => {
      const index = state.list.findIndex(
        (user) => user.id === action.payload.id
      );
      if (index !== -1) {
        state.list[index] = action.payload;
      } else {
        state.list.push(action.payload);
      }
    },
    removeUser: (state, action) => {
      state.list = state.list.filter((user) => user.id !== action.payload);
    },
  },
});

export const {
  fetchUsersStart,
  fetchUsersSuccess,
  fetchUsersFailure,
  upsertUser,
  removeUser,
} = userSlice.actions;

export const selectUsers = (state) => state.users.list;
export const selectUsersLoading = (state) => state.users.loading;
export const selectUsersError = (state) => state.users.error;

export default userSlice.reducer;
