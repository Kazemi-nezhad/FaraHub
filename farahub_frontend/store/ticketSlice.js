import { createSlice } from "@reduxjs/toolkit";

const initialState = {
  list: [],
  selectedTicket: null,
  loading: false,
  error: null,
};

const ticketSlice = createSlice({
  name: "tickets",
  initialState,
  reducers: {
    fetchTicketsStart: (state) => {
      state.loading = true;
      state.error = null;
    },
    fetchTicketsSuccess: (state, action) => {
      state.loading = false;
      state.list = action.payload;
    },
    fetchTicketsFailure: (state, action) => {
      state.loading = false;
      state.error = action.payload;
    },
    selectTicket: (state, action) => {
      state.selectedTicket =
        state.list.find((ticket) => ticket.id === action.payload) || null;
    },
    upsertTicket: (state, action) => {
      const index = state.list.findIndex(
        (ticket) => ticket.id === action.payload.id
      );
      if (index !== -1) {
        state.list[index] = action.payload;
      } else {
        state.list.push(action.payload);
      }
      if (
        state.selectedTicket &&
        state.selectedTicket.id === action.payload.id
      ) {
        state.selectedTicket = action.payload;
      }
    },
    removeTicket: (state, action) => {
      state.list = state.list.filter((ticket) => ticket.id !== action.payload);
      if (state.selectedTicket && state.selectedTicket.id === action.payload) {
        state.selectedTicket = null;
      }
    },
  },
});

export const {
  fetchTicketsStart,
  fetchTicketsSuccess,
  fetchTicketsFailure,
  selectTicket,
  upsertTicket,
  removeTicket,
} = ticketSlice.actions;

export const selectTickets = (state) => state.tickets.list;
export const selectSelectedTicket = (state) => state.tickets.selectedTicket;
export const selectTicketsLoading = (state) => state.tickets.loading;
export const selectTicketsError = (state) => state.tickets.error;

export default ticketSlice.reducer;
