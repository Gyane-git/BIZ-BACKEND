using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IReceiptService
{
    Task<List<ReceiptDto>> GetAllAsync();

    Task<ReceiptDto?> GetByIdAsync(
        int id);

    Task<ReceiptDto?> GetByNumberAsync(
        string receiptNumber);

    Task<List<ReceiptDto>> GetByJournalAsync(
        int journalId);

    Task<ReceiptDto> CreateAsync(
        ReceiptDto dto);

    Task<bool> UpdateAsync(
        int id,
        ReceiptDto dto);

    Task<bool> DeleteAsync(
        int id);
}