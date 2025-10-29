import csv
import random

# --- الإعدادات ---
filename = 'data.csv'         # اسم الملف الذي سيتم إنشاؤه
num_rows = 100                # عدد الصفوف المطلوبة
min_val = 1.0                 # أصغر قيمة عشوائية
max_val = 100.0               # أكبر قيمة عشوائية

# --- الإعدادات الجديدة ---
num_calculation_cols = 5      # عدد الأعمدة التي تدخل في الحساب
num_extra_cols = 10           # عدد الأعمدة الإضافية (لا تدخل في الحساب)
# --------------------

# حساب العدد الإجمالي للأعمدة (باستثناء عمود الحساب)
total_data_cols = num_calculation_cols + num_extra_cols

# 1. تحديد أسماء الأعمدة (Header)
headers = []
# إضافة أعمدة الحساب
for i in range(1, num_calculation_cols + 1):
    headers.append(f'Calc_Column_{i}')
    
# إضافة الأعمدة الإضافية
for i in range(1, num_extra_cols + 1):
    headers.append(f'Extra_Column_{i}')

headers.append('Calculation') # اسم العمود الأخير

# استخدام 'w' للكتابة (سيقوم بإنشاء ملف جديد أو الكتابة فوق ملف قديم)
# نستخدم newline='' لمنع السطور الفارغة الإضافية في ملف CSV
with open(filename, mode='w', newline='', encoding='utf-8') as file:
    writer = csv.writer(file)
    
    # 2. كتابة صف العناوين (Headers)
    writer.writerow(headers)
    
    # 3. البدء بتوليد الصفوف
    for _ in range(num_rows):
        
        # إنشاء قائمة لحفظ بيانات الصف الحالي (للأعمدة الخمسة الأولى فقط)
        calculation_data = []
        
        # 4. توليد الأرقام للأعمدة التي تدخل في الحساب (أول 5)
        for _ in range(num_calculation_cols):
            random_float = random.uniform(min_val, max_val)
            calculation_data.append(round(random_float, 2))
            
        # 5. توليد الأرقام للأعمدة الإضافية (التالية 10)
        extra_data = []
        for _ in range(num_extra_cols):
            # سنستخدم نفس نطاق الأرقام العشوائية (يمكنك تغييره إذا أردت)
            random_float = random.uniform(min_val, max_val)
            extra_data.append(round(random_float, 2))

        # 6. إجراء العملية الحسابية (فقط على الأعمدة الخمسة الأولى)
        # *** التعديل الجوهري هنا: نستخدم calculation_data بدلاً من كل الصف ***
        calculation_result = sum(calculation_data)
        
        # 7. تجميع الصف الكامل (بيانات الحساب + البيانات الإضافية + النتيجة)
        full_row = calculation_data + extra_data + [round(calculation_result, 2)]
        
        # 8. كتابة الصف الكامل في ملف CSV
        writer.writerow(full_row)

print(f"تم إنشاء الملف بنجاح: {filename} ويحتوي على {num_rows} صفاً.")
print(f"كل صف يحتوي على {num_calculation_cols} أعمدة حسابية، {num_extra_cols} أعمدة إضافية، وعمود النتيجة.")