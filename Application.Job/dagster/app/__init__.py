
from dagster import Definitions, define_asset_job, load_assets_from_modules
# from app.assets import recurrence

from app.assets import (
    extract_recurring_transactions,
    create_entries,
    update_recurring_transactions
)

from app.schedules import (
    daily_recurring_transaction_schedule,
    recurring_transaction_schedule_6h,
    custom_recurring_transaction_schedule
)

from app.sensors import (
    recurring_transaction_sensor,
    recurring_transaction_batch_sensor
)



# recurrence_assets = load_assets_from_modules([recurrence])

# Define a job that includes all the recurring transaction assets
recurring_transaction_job = define_asset_job(
    name="recurring_transaction_job",
    selection=[
        extract_recurring_transactions,
        create_entries,
        update_recurring_transactions
    ],
    description="Process recurring transactions and create expense/income entries"
)


defs = Definitions(
    assets=[

    ],
    jobs=[

    ],
    schedules=[

    ],
    sensors=[

    ]
)
