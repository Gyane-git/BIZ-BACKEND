using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetAllAsync();

    Task<PaymentDto?> GetByIdAsync(
        int id);

    Task<PaymentDto?> GetByNumberAsync(
        string paymentNumber);

    Task<List<PaymentDto>> GetByJournalAsync(
        int journalId);

    Task<PaymentDto> CreateAsync(
        PaymentDto dto);

    Task<bool> UpdateAsync(
        int id,
        PaymentDto dto);

    Task<bool> DeleteAsync(
        int id);
}