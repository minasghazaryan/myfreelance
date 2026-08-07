using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Transactions;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Areas.Admin.Pages.Transactions;

public class IndexModel(ITransactionService transactionService) : PageModel
{
    public IReadOnlyList<TransactionListItemDto> Transactions { get; set; } = [];
    public string? Search { get; set; }

    public async Task OnGetAsync(string? search)
    {
        Search = search;
        Transactions = await transactionService.GetAllTransactionsAsync(search: search);
    }
}
