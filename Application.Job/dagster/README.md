# Recurring Transaction Processing - Dagster Assets

This module contains Dagster assets for processing recurring transactions in the budget application.

## Overview

The recurring transaction processing pipeline consists of four main assets:

1. **extract_recurring_transactions** - Extracts active recurring transactions that are due for processing
2. **create_expense_entries** - Creates expense entries from recurring transactions with type "Expense"
3. **create_income_entries** - Creates income entries from recurring transactions with type "Income"  
4. **update_recurring_transactions** - Updates the recurring transactions with new due dates and processed status

## Dependencies

Make sure you have the following dependencies installed:

```bash
pip install dagster dagster-webserver dagster-dg-cli pyodbc pydantic
```

## Configuration

The assets use the `DatabaseConfig` class to configure the database connection. By default, it uses the connection string from the application configuration.

## Running the Assets

### Option 1: Using the Dagster UI

1. Start the Dagster web server:
```bash
cd dagster
dagster-webserver
```

2. Open your browser to `http://localhost:3000`
3. Navigate to the Assets tab
4. Select the recurring transaction assets and click "Materialize"

### Option 2: Using the Python Script

Run the provided script:
```bash
cd dagster
python run_recurring_transactions.py
```

### Option 3: Using Dagster CLI

```bash
cd dagster
dagster asset materialize --select extract_recurring_transactions create_expense_entries create_income_entries update_recurring_transactions
```

## Asset Details

### extract_recurring_transactions

- **Input**: Database connection configuration
- **Output**: List of `RecurringTransactionData` objects
- **Description**: Queries the database for active recurring transactions where `next_due_date <= current_date`

### create_expense_entries

- **Input**: List of recurring transactions
- **Output**: List of created expense entry IDs
- **Description**: Creates expense entries for recurring transactions with type "Expense"

### create_income_entries

- **Input**: List of recurring transactions  
- **Output**: List of created income entry IDs
- **Description**: Creates income entries for recurring transactions with type "Income"

### update_recurring_transactions

- **Input**: List of recurring transactions and created entry IDs
- **Output**: MaterializeResult with metadata
- **Description**: Updates recurring transactions with new due dates and processed timestamps

## Database Schema

The assets work with the following database tables:

- `recurring_transaction` - Source table for recurring transactions
- `expense_entry` - Target table for expense entries
- `income_entry` - Target table for income entries

## Frequency Calculation

The system supports the following frequency types:
- Daily
- Weekly
- BiWeekly
- Monthly
- BiMonthly
- Quarterly
- Yearly
- OneTime

## Error Handling

The assets include proper error handling and transaction management:
- Database transactions are rolled back on errors
- Detailed logging for debugging
- Retry logic can be implemented at the job level

## Monitoring

The assets provide metadata about the processing:
- Number of transactions processed
- Number of expense entries created
- Number of income entries created
- Processing timestamps

## Scheduling

To run this as a scheduled job, you can create a Dagster schedule:

```python
from dagster import schedule, RunRequest

@schedule(
    job=recurring_transaction_job,
    cron_schedule="0 0 * * *"  # Daily at midnight
)
def recurring_transaction_schedule():
    return RunRequest()
```
